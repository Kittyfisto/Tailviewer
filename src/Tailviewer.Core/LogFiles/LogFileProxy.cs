using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using log4net;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Fully represents another <see cref="ILogFile" /> which can be replaced over the lifetime
	///     of the proxy.
	/// </summary>
	/// <remarks>
	///     Exists so that specialized <see cref="ILogFile" /> implementations don't need to be concerned about re-use
	///     or certain changes (i.e. <see cref="FilteredLogFile" /> doesn't need to implement the change of a filter).
	/// </remarks>
	/// <remarks>
	///    Plugin authors are deliberately prevented from instantiating this type directly because it's constructor signature may change
	///    over time. In order to create an instance of this type, simply call <see cref="IServiceContainer.CreateLogFileProxy"/>
	///    who's signature is guaranteed to never change.
	/// </remarks>
	[DebuggerTypeProxy(typeof(LogFileView))]
	internal sealed class LogFileProxy
		: ILogFileProxy
		, ILogFileListener
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ITaskScheduler _taskScheduler;
		private readonly LogFileListenerCollection _listeners;
		private readonly ConcurrentLogFilePropertyCollection _properties;
		private readonly LogFilePropertyList _sourceProperties;
		private readonly ConcurrentQueue<KeyValuePair<ILogFile, LogFileSection>> _pendingSections;
		private readonly IPeriodicTask _task;
		private ILogFile _source;
		private bool _isDisposed;
		private readonly TimeSpan _maximumWaitTime;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="taskScheduler"></param>
		/// <param name="maximumWaitTime"></param>
		public LogFileProxy(ITaskScheduler taskScheduler, TimeSpan maximumWaitTime)
		{
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));

			_taskScheduler = taskScheduler;
			_properties = new ConcurrentLogFilePropertyCollection(LogFileProperties.Minimum);
			_properties.SetValue(LogFileProperties.EmptyReason, ErrorFlags.SourceDoesNotExist);

			_sourceProperties = new LogFilePropertyList();
			_pendingSections = new ConcurrentQueue<KeyValuePair<ILogFile, LogFileSection>>();
			_listeners = new LogFileListenerCollection(this);

			_task = _taskScheduler.StartPeriodic(RunOnce, "Log File Proxy");
			_maximumWaitTime = maximumWaitTime;
		}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <remarks>
		///    Plugin authors are deliberately prevented from calling this constructor directly because it's signature may change
		///    over time. In order to create an instance of this type, simply call <see cref="IServiceContainer.CreateLogFileProxy"/>.
		/// </remarks>
		/// <param name="taskScheduler"></param>
		/// <param name="maximumWaitTime"></param>
		/// <param name="innerLogFile"></param>
		internal LogFileProxy(ITaskScheduler taskScheduler, TimeSpan maximumWaitTime, ILogFile innerLogFile)
			: this(taskScheduler, maximumWaitTime)
		{
			InnerLogFile = innerLogFile;
		}

		private TimeSpan RunOnce()
		{
			bool performedWork = false;

			while (_pendingSections.TryDequeue(out var pair))
			{
				var sender = pair.Key;
				var innerLogFile = _source;
				var section = pair.Value;
				if (sender != innerLogFile)
				{
					// If, for some reason, we receive an event from a previous log file,
					// then we ignore it so our listeners are not confused.
					Log.DebugFormat(
						"Skipping pending modification '{0}' from '{1}' because it is no longer our current log file '{2}'",
						section, sender, innerLogFile);
				}
				else
				{
					if (section.IsReset)
					{
						_listeners.Reset();
					}
					else if (section.IsInvalidate)
					{
						_listeners.Invalidate((int)section.Index, section.Count);
					}
					else
					{
						_listeners.OnRead((int)(section.Index + section.Count));
					}
				}

				performedWork = true;
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
				_properties.SetValue(LogFileProperties.PercentageProcessed, Percentage.HundredPercent);
			}
		}

		/// <inheritdoc />
		public ILogFile InnerLogFile
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
				_pendingSections.Enqueue(new KeyValuePair<ILogFile, LogFileSection>(_source, LogFileSection.Reset));

				_source?.AddListener(this, _maximumWaitTime, 10000);
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			ILogFile logFile = _source;
			logFile?.Dispose();
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
				ILogFile logFile = _source;
				if (logFile != null)
					return logFile.Columns;

				return LogFileColumns.Minimum;
			}
		}

		/// <inheritdoc />
		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
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
		public void RemoveListener(ILogFileListener listener)
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
		public void GetAllProperties(ILogFileProperties destination)
		{
				_properties.CopyAllValuesTo(destination);
		}

		#endregion

		/// <inheritdoc />
		public void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices, IColumnDescriptor<T> column, T[] destination, int destinationIndex, LogFileQueryOptions queryOptions)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));

			ILogFile logFile = _source;
			if (logFile != null)
			{
				logFile.GetColumn(sourceIndices, column, destination, destinationIndex, queryOptions);
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
		public void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogEntries destination, int destinationIndex, LogFileQueryOptions queryOptions)
		{
			ILogFile logFile = _source;
			if (logFile != null)
			{
				logFile.GetEntries(sourceIndices, destination, destinationIndex, queryOptions);
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
		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			if (Log.IsDebugEnabled)
				Log.DebugFormat("OnLogFileModified({0}, {1})", logFile, section);

			_pendingSections.Enqueue(new KeyValuePair<ILogFile, LogFileSection>(logFile, section));
		}
	}
}