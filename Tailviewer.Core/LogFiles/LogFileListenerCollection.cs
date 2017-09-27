using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Holds a list of zero or more <see cref="ILogFileListener" /> and forwards events to every single listener,
	///     according to the settings given to <see cref="AddListener" />.
	/// </summary>
	/// <remarks>
	///     This class should definately be used when implementing the <see cref="ILogFile" /> interface.
	/// </remarks>
	public sealed class LogFileListenerCollection
	{
		private readonly Dictionary<ILogFileListener, LogFileListenerNotifier> _listeners;
		private readonly ILogFile _logFile;

		/// <summary>
		///     Initializes this collection.
		/// </summary>
		/// <param name="logFile">
		///     The log file that should be forwarded to <see cref="ILogFileListener.OnLogFileModified" />
		/// </param>
		public LogFileListenerCollection(ILogFile logFile)
		{
			if (logFile == null) throw new ArgumentNullException(nameof(logFile));

			_logFile = logFile;
			_listeners = new Dictionary<ILogFileListener, LogFileListenerNotifier>();
			CurrentLineIndex = -1;
		}

		/// <summary>
		///     The highest index that's been reported via <see cref="OnRead" />.
		///     Set to -1 after construction and <see cref="Reset" />.
		/// </summary>
		public int CurrentLineIndex { get; private set; }

		/// <summary>
		///     Adds the given listener to the list of notified listeners.
		///     The listener will immediately be notified of the <see cref="CurrentLineIndex" />.
		/// </summary>
		/// <param name="listener"></param>
		/// <param name="maximumWaitTime"></param>
		/// <param name="maximumLineCount"></param>
		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			lock (_listeners)
			{
				if (!_listeners.ContainsKey(listener))
				{
					var notifier = new LogFileListenerNotifier(_logFile, listener, maximumWaitTime, maximumLineCount);
					_listeners.Add(listener, notifier);
					notifier.OnRead(CurrentLineIndex);
				}
			}
		}

		/// <summary>
		///     Removes the given listener from the list of notified listeners.
		/// </summary>
		/// <param name="listener"></param>
		public void RemoveListener(ILogFileListener listener)
		{
			lock (_listeners)
			{
				_listeners.Remove(listener);
			}
		}

		/// <summary>
		///     Resets <see cref="CurrentLineIndex" /> to -1 and notifies all listeners.
		/// </summary>
		public void Reset()
		{
			OnRead(-1);
		}

		/// <summary>
		///     Sets <see cref="CurrentLineIndex" /> to the given number of lines read and
		///     notifies all listeners (in case their maximumNumberOfLines is reached or maximumWaitTime elapsed).
		/// </summary>
		/// <remarks>
		///     Please make sure to call this method regularly, so that a listener which wasn't notified previously,
		///     is eventually.
		/// </remarks>
		/// <param name="numberOfLinesRead"></param>
		public void OnRead(int numberOfLinesRead)
		{
			lock (_listeners)
			{
				foreach (var notifier in _listeners.Values)
					notifier.OnRead(numberOfLinesRead);
				CurrentLineIndex = numberOfLinesRead;
			}
		}

		/// <summary>
		///     Invalidates the given region of log lines.
		/// </summary>
		/// <remarks>
		///     Make sure to always invalidate a section until the very end of the log file.
		/// </remarks>
		/// <param name="firstIndex"></param>
		/// <param name="count"></param>
		public void Invalidate(int firstIndex, int count)
		{
			lock (_listeners)
			{
				foreach (var notifier in _listeners.Values)
					notifier.Invalidate(firstIndex, count);
				CurrentLineIndex = firstIndex;
			}
		}

		/// <summary>
		///     Ensures that all listeners are notified of the latest changes, even if not enough time
		///     elapsed according to their maximumWaitTime.
		/// </summary>
		public void Flush()
		{
			lock (_listeners)
			{
				foreach (var notifier in _listeners.Values)
					notifier.Flush(CurrentLineIndex, DateTime.Now);
			}
		}
	}
}