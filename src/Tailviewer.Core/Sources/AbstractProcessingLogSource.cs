﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using log4net;
using Tailviewer.Core.Buffers;
using Tailviewer.Core.Properties;

namespace Tailviewer.Core.Sources
{
	/// <summary>
	///     The base class for a log source which processes the log entries from another source
	///     as they arrive. Takes care of most of the plumbing so the implementation can take
	///     care of the actual processing.
	/// </summary>
	/// <remarks>
	///     The source is processed in fixed size blocks and abstract methods are called
	///     depending on what has transpired.
	/// </remarks>
	internal abstract class AbstractProcessingLogSource
		: AbstractLogSource
		, ILogSourceListener
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly LogBufferArray _fetchBuffer;
		private readonly int _maxEntryCount;
		private readonly TimeSpan _maximumWaitTime;
		private readonly ConcurrentQueue<LogFileSection> _pendingSections;
		private readonly ConcurrentPropertiesList _properties;
		private readonly PropertiesBufferList _propertiesBuffer;
		private readonly PropertiesBufferHidingView _propertiesBufferView;
		private readonly ILogSource _source;
		private int _count;

		protected AbstractProcessingLogSource(ITaskScheduler taskScheduler,
		                                      ILogSource source,
		                                      IReadOnlyList<IColumnDescriptor> columnsToFetch,
		                                      IReadOnlyList<IReadOnlyPropertyDescriptor> overwrittenProperties,
		                                      TimeSpan maximumWaitTime,
		                                      int maxEntryCount = 1000)
			: base(taskScheduler)
		{
			_fetchBuffer = new LogBufferArray(maxEntryCount, columnsToFetch);
			_pendingSections = new ConcurrentQueue<LogFileSection>();
			_source = source;
			_maximumWaitTime = maximumWaitTime;
			_maxEntryCount = maxEntryCount;
			_propertiesBuffer = new PropertiesBufferList();
			_propertiesBufferView = new PropertiesBufferHidingView(_propertiesBuffer, overwrittenProperties);
			_properties = new ConcurrentPropertiesList();
		}

		#region Implementation of ILogSourceListener

		public void OnLogFileModified(ILogSource logSource, LogFileSection section)
		{
			_pendingSections.Enqueue(section);
		}

		#endregion

		private bool Process(IReadOnlyList<LogFileSection> pendingSections)
		{
			if (pendingSections.Count == 0)
				return false;

			foreach (var section in pendingSections)
				if (section.IsReset)
					Reset();
				else if (section.IsInvalidate)
					InvalidateSection(section);
				else
					AppendSection(section);

			return true;
		}

		private void AppendSection(LogFileSection section)
		{
			_count = (int) (section.Index + section.Count);
			try
			{
				_source.GetEntries(section, _fetchBuffer);
				OnAppendSection(section, _fetchBuffer, _count);
			}
			catch (Exception e)
			{
				Log.WarnFormat("Caught unexpected exception: {0}", e);
			}

			SynchronizeProperties();
			Listeners.OnRead(_count);
		}

		private void InvalidateSection(LogFileSection section)
		{
			_count = (int) section.Index;
			try
			{
				OnInvalidateSection(_count);
			}
			catch (Exception e)
			{
				Log.WarnFormat("Caught unexpected exception: {0}", e);
			}

			SynchronizeProperties();
			Listeners.Invalidate((int) section.Index, section.Count);
		}

		private void Reset()
		{
			_count = 0;
			try
			{
				OnResetSection();
			}
			catch (Exception e)
			{
				Log.WarnFormat("Caught unexpected exception: {0}", e);
			}

			SynchronizeProperties();
			Listeners.Reset();
		}

		protected abstract void OnResetSection();

		protected abstract void OnInvalidateSection(int totalLogEntryCount);

		protected abstract void OnAppendSection(LogFileSection section,
		                                        IReadOnlyLogBuffer data,
		                                        int totalLogEntryCount);

		protected abstract void NothingToProcess();

		/// <summary>
		///     This method is called when properties are synchronized with the clients of this class.
		///     If the implementation of this class has some overwritten properties, then now is the time to write them
		///     into the <paramref name="destination" /> buffer.
		/// </summary>
		/// <param name="destination"></param>
		protected abstract void GetOverwrittenProperties(PropertiesBufferList destination);

		private void SynchronizeProperties()
		{
			_source.GetAllProperties(_propertiesBufferView);

			var sourceProcessed = _propertiesBuffer.GetValue(GeneralProperties.PercentageProcessed);
			var sourceCount = _propertiesBuffer.GetValue(GeneralProperties.LogEntryCount);
			var ownProgress = sourceCount > 0
				? Percentage.Of(_count, sourceCount).Clamped()
				: Percentage.HundredPercent;
			var totalProgress = (sourceProcessed * ownProgress).Clamped();

			_propertiesBuffer.SetValue(GeneralProperties.PercentageProcessed, totalProgress);
			_propertiesBuffer.SetValue(GeneralProperties.LogEntryCount, _count);

			GetOverwrittenProperties(_propertiesBuffer);

			_properties.CopyFrom(_propertiesBuffer);
		}

		#region Overrides of AbstractLogSource

		protected override void StartTask()
		{
			_source.AddListener(this, _maximumWaitTime, _maxEntryCount);
			base.StartTask();
		}

		protected sealed override TimeSpan RunOnce(CancellationToken token)
		{
			var pendingSections = _pendingSections.DequeueAll();
			if (!Process(pendingSections))
			{
				NothingToProcess();
				SynchronizeProperties();
				Listeners.OnRead(_count);
			}

			return _maximumWaitTime;
		}

		#endregion

		#region Overrides of AbstractLogSource

		public sealed override IReadOnlyList<IColumnDescriptor> Columns
		{
			get { return _source.Columns; }
		}

		public sealed override IReadOnlyList<IReadOnlyPropertyDescriptor> Properties
		{
			get { return _properties.Properties; }
		}

		public sealed override object GetProperty(IReadOnlyPropertyDescriptor property)
		{
			return _properties.GetValue(property);
		}

		public sealed override T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			return _properties.GetValue(property);
		}

		public sealed override void SetProperty(IPropertyDescriptor property, object value)
		{
			_source.SetProperty(property, value);
		}

		public sealed override void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			_source.SetProperty(property, value);
		}

		public sealed override void GetAllProperties(IPropertiesBuffer destination)
		{
			_properties.CopyAllValuesTo(destination);
		}

		protected override void DisposeAdditional()
		{
			_source.RemoveListener(this);

			base.DisposeAdditional();
		}

		#endregion
	}
}