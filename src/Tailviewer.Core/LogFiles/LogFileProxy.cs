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
		private readonly ConcurrentQueue<KeyValuePair<ILogFile, LogFileSection>> _pendingSections;
		private readonly IPeriodicTask _task;
		private ILogFile _innerLogFile;
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

			KeyValuePair<ILogFile, LogFileSection> pair;
			while (_pendingSections.TryDequeue(out pair))
			{
				var sender = pair.Key;
				var innerLogFile = _innerLogFile;
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

			// This line is extremely important because listeners are allowed to limit how often they are notified.
			// This means that even when there is NO modification to the source, we still need to notify the collection
			// so it can check if enough time has ellapsed to finally notify listener.
			_listeners.OnRead(_listeners.CurrentLineIndex);

			if (performedWork)
				return TimeSpan.Zero;

			return TimeSpan.FromMilliseconds(10);
		}

		/// <inheritdoc />
		public ILogFile InnerLogFile
		{
			get { return _innerLogFile; }
			set
			{
				if (value == _innerLogFile)
					return;

				_innerLogFile?.RemoveListener(this);

				_innerLogFile = value;

				// We're now representing a different log file.
				// To the outside, we model this as a simple reset, followed
				// by the content of the new logfile...
				_pendingSections.Enqueue(new KeyValuePair<ILogFile, LogFileSection>(_innerLogFile, LogFileSection.Reset));

				_innerLogFile?.AddListener(this, _maximumWaitTime, 10000);
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			ILogFile logFile = _innerLogFile;
			logFile?.Dispose();
			_taskScheduler.StopPeriodic(_task);
			_isDisposed = true;
		}

		/// <summary>
		///     Whether or not <see cref="Dispose" /> has been called already.
		/// </summary>
		public bool IsDisposed => _isDisposed;

		/// <inheritdoc />
		public bool EndOfSourceReached
		{
			get
			{
				ILogFile logFile = _innerLogFile;
				if (logFile != null)
					return logFile.EndOfSourceReached;

				return false;
			}
		}

		/// <inheritdoc />
		public int Count
		{
			get
			{
				ILogFile logFile = _innerLogFile;
				if (logFile != null)
					return logFile.Count;

				return 0;
			}
		}

		/// <inheritdoc />
		public int OriginalCount
		{
			get
			{
				ILogFile logFile = _innerLogFile;
				if (logFile != null)
					return logFile.OriginalCount;

				return 0;
			}
		}

		/// <inheritdoc />
		public int MaxCharactersPerLine
		{
			get
			{
				ILogFile logFile = _innerLogFile;
				if (logFile != null)
					return logFile.MaxCharactersPerLine;

				return 0;
			}
		}

		/// <inheritdoc />
		public IReadOnlyList<ILogFileColumn> Columns
		{
			get
			{
				ILogFile logFile = _innerLogFile;
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
			var logFile = _innerLogFile;
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
		public IReadOnlyList<ILogFilePropertyDescriptor> Properties => _innerLogFile?.Properties ?? LogFileProperties.Minimum;

		/// <inheritdoc />
		public object GetValue(ILogFilePropertyDescriptor propertyDescriptor)
		{
			var logFile = _innerLogFile;
			if (logFile != null)
				return logFile.GetValue(propertyDescriptor);

			if (Equals(propertyDescriptor, LogFileProperties.EmptyReason))
				return ErrorFlags.SourceDoesNotExist;

			return propertyDescriptor.DefaultValue;
		}

		/// <inheritdoc />
		public T GetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor)
		{
			var logFile = _innerLogFile;
			if (logFile != null)
				return logFile.GetValue(propertyDescriptor);

			if (Equals(propertyDescriptor, LogFileProperties.EmptyReason))
				return (T)(object)ErrorFlags.SourceDoesNotExist;

			return propertyDescriptor.DefaultValue;
		}

		/// <inheritdoc />
		public void GetValues(ILogFileProperties properties)
		{
			var logFile = _innerLogFile;
			if (logFile != null)
			{
				logFile.GetValues(properties);
			}
			else
			{
				foreach (var descriptor in properties.Properties)
				{
					if (Equals(descriptor, LogFileProperties.EmptyReason))
						properties.SetValue(descriptor, ErrorFlags.SourceDoesNotExist);
					else
						properties.SetValue(descriptor, descriptor.DefaultValue);
				}
			}
		}

		#endregion

		/// <inheritdoc />
		public void GetColumn<T>(LogFileSection sourceSection, ILogFileColumn<T> column, T[] destination, int destinationIndex)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));
			if (destinationIndex + sourceSection.Count > destination.Length)
				throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");

			ILogFile logFile = _innerLogFile;
			if (logFile != null)
			{
				logFile.GetColumn(sourceSection, column, destination, destinationIndex);
			}
			else
			{
				destination.Fill(column.DefaultValue, destinationIndex, sourceSection.Count);
			}
		}

		/// <inheritdoc />
		public void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices, ILogFileColumn<T> column, T[] destination, int destinationIndex)
		{
			if (sourceIndices == null)
				throw new ArgumentNullException(nameof(sourceIndices));
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));
			if (destinationIndex + sourceIndices.Count > destination.Length)
				throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");

			ILogFile logFile = _innerLogFile;
			if (logFile != null)
			{
				logFile.GetColumn(sourceIndices, column, destination, destinationIndex);
			}
			else
			{
				destination.Fill(column.DefaultValue, destinationIndex, sourceIndices.Count);
			}
		}

		/// <inheritdoc />
		public void GetEntries(LogFileSection sourceSection, ILogEntries destination, int destinationIndex)
		{
			ILogFile logFile = _innerLogFile;
			if (logFile != null)
			{
				logFile.GetEntries(sourceSection, destination, destinationIndex);
			}
			else
			{
				foreach (var column in destination.Columns)
				{
					destination.FillDefault(column, destinationIndex, sourceSection.Count);
				}
			}
		}

		/// <inheritdoc />
		public void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogEntries destination, int destinationIndex)
		{
			ILogFile logFile = _innerLogFile;
			if (logFile != null)
			{
				logFile.GetEntries(sourceIndices, destination, destinationIndex);
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
		public void GetSection(LogFileSection section, LogLine[] dest)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			var logFile = _innerLogFile;
			if (logFile != null)
			{
				return logFile.GetLogLineIndexOfOriginalLineIndex(originalLineIndex);
			}

			return LogLineIndex.Invalid;
		}

		/// <inheritdoc />
		public LogLine GetLine(int index)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public double Progress => _innerLogFile?.Progress ?? 1;

		/// <inheritdoc />
		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			if (Log.IsDebugEnabled)
				Log.DebugFormat("OnLogFileModified({0}, {1})", logFile, section);

			_pendingSections.Enqueue(new KeyValuePair<ILogFile, LogFileSection>(logFile, section));
		}
	}
}