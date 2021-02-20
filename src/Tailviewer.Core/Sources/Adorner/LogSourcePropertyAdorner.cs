using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tailviewer.Core.Buffers;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Properties;

namespace Tailviewer.Core.Sources.Adorner
{
	/// <summary>
	///     This class is responsible for calculating the values for certain properties based on the data of an underlying
	///     <see cref="ILogSource" />.
	/// </summary>
	/// <remarks>
	///     This class can only adorn properties for which it knows how to calculate them.
	/// </remarks>
	internal sealed class LogSourcePropertyAdorner
		: AbstractLogSource
		, ILogSourceListener
	{
		public static readonly IReadOnlyList<IReadOnlyPropertyDescriptor> AllAdornedProperties;

		private readonly ILogSource _source;
		private readonly TimeSpan _maximumWaitTime;
		private readonly IReadOnlyList<IReadOnlyPropertyDescriptor> _adornedProperties;
		private readonly IReadOnlyList<IColumnDescriptor> _requiredColumns;
		private readonly PropertiesBufferList _propertiesBuffer;
		private readonly PropertiesBufferList _sourceProperties;
		private readonly ConcurrentPropertiesList _properties;
		private readonly ConcurrentQueue<LogFileSection> _pendingSections;
		private readonly LogBufferArray _buffer;
		private int _count;

		static LogSourcePropertyAdorner()
		{
			AllAdornedProperties = new IReadOnlyPropertyDescriptor[]
			{
				GeneralProperties.StartTimestamp,
				GeneralProperties.EndTimestamp,
				GeneralProperties.Duration
			};
		}

		private static IReadOnlyList<IColumnDescriptor> GetColumnsRequiredFor(IReadOnlyList<IReadOnlyPropertyDescriptor> adornedProperties)
		{
			var requiredColumns = new List<IColumnDescriptor>
			{
				GeneralColumns.Index
			};
			if (adornedProperties.Contains(GeneralProperties.StartTimestamp) ||
			    adornedProperties.Contains(GeneralProperties.EndTimestamp) ||
			    adornedProperties.Contains(GeneralProperties.Duration))
				requiredColumns.Add(GeneralColumns.Timestamp);

			return requiredColumns;
		}

		public LogSourcePropertyAdorner(ITaskScheduler scheduler, ILogSource source, TimeSpan maximumWaitTime)
			: this(scheduler, source, maximumWaitTime, AllAdornedProperties)
		{ }

		public LogSourcePropertyAdorner(ITaskScheduler scheduler, ILogSource source, TimeSpan maximumWaitTime, IReadOnlyList<IReadOnlyPropertyDescriptor> adornedProperties)
			: base(scheduler)
		{
			_source = source;
			_maximumWaitTime = maximumWaitTime;
			_adornedProperties = adornedProperties;
			foreach (var adornedProperty in _adornedProperties)
			{
				if (!AllAdornedProperties.Contains(adornedProperty))
					throw new ArgumentException($"This log source doesn't support adorning '{adornedProperty}'!");
			}

			_propertiesBuffer = new PropertiesBufferList(_adornedProperties);
			_sourceProperties = new PropertiesBufferList();
			_properties = new ConcurrentPropertiesList(_adornedProperties);
			_pendingSections = new ConcurrentQueue<LogFileSection>();
			_requiredColumns = GetColumnsRequiredFor(_adornedProperties);
			const int bufferSize = 1000;
			_buffer = new LogBufferArray(bufferSize, _requiredColumns);

			_source.AddListener(this, maximumWaitTime, bufferSize);
			StartTask();
		}

		#region Overrides of AbstractLogSource

		public override IReadOnlyList<IColumnDescriptor> Columns => _source.Columns;

		public override IReadOnlyList<IReadOnlyPropertyDescriptor> Properties => _source.Properties.Concat(_adornedProperties).ToList();

		public override object GetProperty(IReadOnlyPropertyDescriptor property)
		{
			if (_properties.Contains(property))
				return _properties.GetValue(property);
			return _source.GetProperty(property);
		}

		public override T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			if (_properties.Contains(property))
				return _properties.GetValue(property);
			return _source.GetProperty(property);
		}

		public override void SetProperty(IPropertyDescriptor property, object value)
		{
			_source.SetProperty(property, value);
		}

		public override void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			_source.SetProperty(property, value);
		}

		public override void GetAllProperties(IPropertiesBuffer destination)
		{
			_source.GetAllProperties(destination);
			_properties.CopyAllValuesTo(destination);
		}

		public override void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices,
		                                  IColumnDescriptor<T> column,
		                                  T[] destination,
		                                  int destinationIndex,
		                                  LogSourceQueryOptions queryOptions)
		{
			_source.GetColumn(sourceIndices, column, destination, destinationIndex, queryOptions);
		}

		public override void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices,
		                                ILogBuffer destination,
		                                int destinationIndex,
		                                LogSourceQueryOptions queryOptions)
		{
			_source.GetEntries(sourceIndices, destination, destinationIndex, queryOptions);
		}

		protected override TimeSpan RunOnce(CancellationToken token)
		{
			var pendingSections = _pendingSections.DequeueAll();
			Process(pendingSections);

			return _maximumWaitTime;
		}

		#endregion

		private void Process(IReadOnlyList<LogFileSection> pendingSections)
		{
			foreach (var section in pendingSections)
			{
				if (section.IsReset)
				{
					_count = 0;
					Listeners.Reset();
					_propertiesBuffer.SetToDefault(_adornedProperties);
					SynchronizeProperties();
				}
				else if (section.IsInvalidate)
				{
					_count = (int) section.Index;
					SynchronizeProperties();
					Listeners.Invalidate((int) section.Index, section.Count);
				}
				else
				{
					Process(section);
					_count += section.Count;
					SynchronizeProperties();
					Listeners.OnRead(_count);
				}
			}
		}

		private void Process(LogFileSection section)
		{
			DateTime? startTime = _propertiesBuffer.GetValue(GeneralProperties.StartTimestamp);
			DateTime? endTime = _propertiesBuffer.GetValue(GeneralProperties.EndTimestamp);
			_source.GetEntries(section, _buffer, 0, LogSourceQueryOptions.Default);
			foreach (var entry in _buffer)
			{
				if (!entry.Index.IsValid)
					break;

				var timestamp = entry.Timestamp;
				if (timestamp != null)
				{
					if (startTime == null)
						startTime = timestamp;
					else if (timestamp < startTime)
						startTime = timestamp;

					if (endTime == null)
						endTime = timestamp;
					else if (timestamp > endTime)
						endTime = timestamp;
				}
			}

			_propertiesBuffer.SetValue(GeneralProperties.StartTimestamp, startTime);
			_propertiesBuffer.SetValue(GeneralProperties.EndTimestamp, endTime);
			_propertiesBuffer.SetValue(GeneralProperties.Duration, endTime - startTime);
		}

		private void SynchronizeProperties()
		{
			_source.GetAllProperties(_sourceProperties);

			var sourceProcessed = _sourceProperties.GetValue(GeneralProperties.PercentageProcessed);
			var sourceCount = _sourceProperties.GetValue(GeneralProperties.LogEntryCount);
			var ownProgress = sourceCount > 0
				? Percentage.Of(_count, sourceCount).Clamped()
				: Percentage.HundredPercent;
			var totalProgress = (sourceProcessed * ownProgress).Clamped();
			_propertiesBuffer.SetValue(GeneralProperties.PercentageProcessed, totalProgress);
			_properties.CopyFrom(_propertiesBuffer);
		}

		#region Implementation of ILogSourceListener

		public void OnLogFileModified(ILogSource logSource, LogFileSection section)
		{
			_pendingSections.Enqueue(section);
		}

		#endregion
	}
}
