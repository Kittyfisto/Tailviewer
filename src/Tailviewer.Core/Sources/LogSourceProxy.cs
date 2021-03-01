using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using log4net;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Properties;

namespace Tailviewer.Core.Sources
{
	/// <summary>
	///     Fully represents another <see cref="ILogSource" /> which can be replaced over the lifetime
	///     of the proxy.
	/// </summary>
	/// <remarks>
	///     Exists so that specialized <see cref="ILogSource" /> implementations don't need to be concerned about re-use
	///     or certain changes (i.e. <see cref="FilteredLogSource" /> doesn't need to implement the change of a filter).
	/// </remarks>
	/// <remarks>
	///    Plugin authors are deliberately prevented from instantiating this type directly because it's constructor signature may change
	///    over time. In order to create an instance of this type, simply call <see cref="ILogSourceFactory.CreateLogFileProxy"/>
	///    who's signature is guaranteed to never change.
	/// </remarks>
	[DebuggerTypeProxy(typeof(LogSourceDebuggerVisualization))]
	internal sealed class LogSourceProxy
		: ILogSourceProxy
		, ILogSourceListener
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public const int DefaultMaxEntryCount = 10000;

		private readonly ITaskScheduler _taskScheduler;
		private readonly LogSourceListenerCollection _listeners;
		private readonly ConcurrentPropertiesList _properties;
		private readonly PropertiesBufferList _sourceProperties;
		private readonly ConcurrentQueue<KeyValuePair<ILogSource, LogSourceModification>> _pendingSections;
		private readonly IPeriodicTask _task;
		private ILogSource _source;
		private bool _isDisposed;
		private readonly TimeSpan _maximumWaitTime;
		private readonly int _maxEntryCount;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="taskScheduler"></param>
		/// <param name="maximumWaitTime"></param>
		/// <param name="maxEntryCount"></param>
		public LogSourceProxy(ITaskScheduler taskScheduler, TimeSpan maximumWaitTime, int maxEntryCount = DefaultMaxEntryCount)
		{
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));

			_taskScheduler = taskScheduler;
			_properties = new ConcurrentPropertiesList(GeneralProperties.Minimum);
			_properties.SetValue(GeneralProperties.EmptyReason, ErrorFlags.SourceDoesNotExist);

			_sourceProperties = new PropertiesBufferList();
			_pendingSections = new ConcurrentQueue<KeyValuePair<ILogSource, LogSourceModification>>();
			_listeners = new LogSourceListenerCollection(this);

			_task = _taskScheduler.StartPeriodic(RunOnce, "Log File Proxy");
			_maximumWaitTime = maximumWaitTime;
			_maxEntryCount = maxEntryCount;
		}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <remarks>
		///    Plugin authors are deliberately prevented from calling this constructor directly because it's signature may change
		///    over time. In order to create an instance of this type, simply call <see cref="ILogSourceFactory.CreateLogFileProxy"/>.
		/// </remarks>
		/// <param name="taskScheduler"></param>
		/// <param name="maximumWaitTime"></param>
		/// <param name="innerLogSource"></param>
		/// <param name="maxEntryCount"></param>
		internal LogSourceProxy(ITaskScheduler taskScheduler, TimeSpan maximumWaitTime, ILogSource innerLogSource, int maxEntryCount = DefaultMaxEntryCount)
			: this(taskScheduler, maximumWaitTime, maxEntryCount)
		{
			InnerLogSource = innerLogSource;
		}

		private TimeSpan RunOnce()
		{
			bool performedWork = false;

			//while (_pendingSections.TryDequeue(out var pair))
			if (_pendingSections.TryDequeueUpTo(_maxEntryCount, out var pendingModifications))
			{
				foreach(var pair in pendingModifications)
				{
					var sender = pair.Key;
					var innerLogFile = _source;
					var modification = pair.Value;
					if (sender != innerLogFile)
					{
						// If, for some reason, we receive an event from a previous log file,
						// then we ignore it so our listeners are not confused.
						Log.DebugFormat(
						                "Skipping pending modification '{0}' from '{1}' because it is no longer our current log file '{2}'",
						                modification, sender, innerLogFile);
					}
					else
					{
						if (modification.IsReset())
						{
							_listeners.Reset();
						}
						else if (modification.IsRemoved(out var removedSection))
						{
							_listeners.Invalidate((int)removedSection.Index, removedSection.Count);
						}
						else if (modification.IsAppended(out var appendedSection))
						{
							_listeners.OnRead((int)(appendedSection.Index + appendedSection.Count));
						}
					}

					performedWork = true;
				}
			}

			UpdateProperties();

			// This line is extremely important because listeners are allowed to limit how often they are notified.
			// This means that even when there is NO modification to the source, we still need to notify the collection
			// so it can check if enough time has elapsed to finally notify listener.
			_listeners.OnRead(_listeners.CurrentLineIndex);

			if (performedWork)
				return TimeSpan.Zero;

			return TimeSpan.FromMilliseconds(10);
		}

		private void UpdateProperties()
		{
			if (_source != null)
			{
				_source.GetAllProperties(_sourceProperties);
				_properties.CopyFrom(_sourceProperties);
			}
			else
			{
				_properties.SetValue(GeneralProperties.PercentageProcessed, Percentage.HundredPercent);
			}
		}

		/// <inheritdoc />
		public ILogSource InnerLogSource
		{
			get { return _source; }
			set
			{
				if (value == _source)
					return;

				_source?.RemoveListener(this);

				_source = value;

				_properties.Reset();

				// We're now representing a different log file.
				// To the outside, we model this as a simple reset, followed
				// by the content of the new logfile...
				_pendingSections.Enqueue(new KeyValuePair<ILogSource, LogSourceModification>(_source, LogSourceModification.Reset()));

				_source?.AddListener(this, _maximumWaitTime, 10000);
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			ILogSource logSource = _source;
			logSource?.Dispose();
			_taskScheduler.StopPeriodic(_task);

			// https://github.com/Kittyfisto/Tailviewer/issues/282
			_listeners.Clear();

			_properties.Clear();

			_source = null;
			_isDisposed = true;
		}

		/// <summary>
		///     Whether or not <see cref="Dispose" /> has been called already.
		/// </summary>
		public bool IsDisposed => _isDisposed;

		/// <inheritdoc />
		public IReadOnlyList<IColumnDescriptor> Columns
		{
			get
			{
				ILogSource logSource = _source;
				if (logSource != null)
					return logSource.Columns;

				return GeneralColumns.Minimum;
			}
		}

		/// <inheritdoc />
		public void AddListener(ILogSourceListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			if (Log.IsDebugEnabled)
				Log.DebugFormat("AddListener({0}, {1}, {2})", listener, maximumWaitTime, maximumLineCount);

			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			var logFile = _source;
			if (logFile != null)
				return string.Format("{0} (Proxy)", logFile);

			return "<Empty>";
		}

		/// <inheritdoc />
		public void RemoveListener(ILogSourceListener listener)
		{
			if (Log.IsDebugEnabled)
				Log.DebugFormat("RemoveListener({0})", listener);

			_listeners.RemoveListener(listener);
		}

		#region Properties

		/// <inheritdoc />
		public IReadOnlyList<IReadOnlyPropertyDescriptor> Properties
		{
			get
			{
				return _properties.Properties;
			}
		}

		/// <inheritdoc />
		public object GetProperty(IReadOnlyPropertyDescriptor property)
		{
			_properties.TryGetValue(property, out var value);
			return value;
		}

		/// <inheritdoc />
		public T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			_properties.TryGetValue(property, out var value);
			return value;
		}

		public void SetProperty(IPropertyDescriptor property, object value)
		{
			_properties.SetValue(property, value);
		}

		public void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			_properties.SetValue(property, value);
		}

		/// <inheritdoc />
		public void GetAllProperties(IPropertiesBuffer destination)
		{
				_properties.CopyAllValuesTo(destination);
		}

		#endregion

		/// <inheritdoc />
		public void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices, IColumnDescriptor<T> column, T[] destination, int destinationIndex, LogSourceQueryOptions queryOptions)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));

			ILogSource logSource = _source;
			if (logSource != null)
			{
				logSource.GetColumn(sourceIndices, column, destination, destinationIndex, queryOptions);
			}
			else
			{
				if (sourceIndices == null)
					throw new ArgumentNullException(nameof(sourceIndices));
				if (destinationIndex < 0)
					throw new ArgumentOutOfRangeException(nameof(destinationIndex));
				if (destinationIndex + sourceIndices.Count > destination.Length)
					throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");

				destination.Fill(column.DefaultValue, destinationIndex, sourceIndices.Count);
			}
		}

		/// <inheritdoc />
		public void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogBuffer destination, int destinationIndex, LogSourceQueryOptions queryOptions)
		{
			ILogSource logSource = _source;
			if (logSource != null)
			{
				logSource.GetEntries(sourceIndices, destination, destinationIndex, queryOptions);
			}
			else
			{
				foreach (var column in destination.Columns)
				{
					destination.FillDefault(column, destinationIndex, sourceIndices.Count);
				}
			}
		}

		/// <inheritdoc />
		public LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			var logFile = _source;
			if (logFile != null)
			{
				return logFile.GetLogLineIndexOfOriginalLineIndex(originalLineIndex);
			}

			return LogLineIndex.Invalid;
		}

		/// <inheritdoc />
		public void OnLogFileModified(ILogSource logSource, LogSourceModification modification)
		{
			if (Log.IsDebugEnabled)
				Log.DebugFormat("OnLogFileModified({0}, {1})", logSource, modification);

			_pendingSections.Enqueue(new KeyValuePair<ILogSource, LogSourceModification>(logSource, modification));
		}
	}
}