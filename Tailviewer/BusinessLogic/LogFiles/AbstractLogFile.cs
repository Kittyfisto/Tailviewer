using System;
using System.Reflection;
using System.Threading;
using log4net;
using Metrolib;

namespace Tailviewer.BusinessLogic.LogFiles
{
	public abstract class AbstractLogFile
		: ILogFile
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ITaskScheduler _scheduler;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly LogFileListenerCollection _listeners;
		private IPeriodicTask _readTask;
		private bool _isDisposed;
		private volatile bool _endOfSourceReached;

		protected AbstractLogFile(ITaskScheduler scheduler)
		{
			if (scheduler == null)
				throw new ArgumentNullException(nameof(scheduler));

			_scheduler = scheduler;
			_cancellationTokenSource = new CancellationTokenSource();
			_listeners = new LogFileListenerCollection(this);
		}

		protected LogFileListenerCollection Listeners => _listeners;

		public bool IsDisposed => _isDisposed;

		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void RemoveListener(ILogFileListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		public void Dispose()
		{
			try
			{
				DisposeAdditional();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
			_cancellationTokenSource.Cancel();
			_isDisposed = true;
			_scheduler.StopPeriodic(_readTask);
		}

		protected virtual void DisposeAdditional()
		{}

		public abstract int MaxCharactersPerLine { get; }

		public abstract bool Exists { get; }

		public virtual bool EndOfSourceReached => _endOfSourceReached;

		public abstract DateTime? StartTimestamp { get; }
		public abstract DateTime LastModified { get; }
		public abstract Size FileSize { get; }
		public abstract int Count { get; }

		public abstract void GetSection(LogFileSection section, LogLine[] dest);

		public virtual LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			return originalLineIndex;
		}

		public abstract LogLine GetLine(int index);

		protected abstract TimeSpan RunOnce(CancellationToken token);

		private TimeSpan Run()
		{
			CancellationToken token = _cancellationTokenSource.Token;
			return RunOnce(token);
		}

		protected void SetEndOfSourceReached()
		{
			// Now this line is very important:
			// Most tests expect that listeners have been notified
			// of all pending changes when the source enters the
			// "EndOfSourceReached" state. This would be true, if not
			// for listeners specifying a timespan that should ellapse between
			// calls to OnLogFileModified. The listener collection has
			// been notified, but the individual listeners may not be, because
			// neither the maximum line count, nor the maximum timespan has ellapsed.
			// Therefore we flush the collection to ensure that ALL listeners have been notified
			// of ALL changes (even if they didn't want them yet) before we enter the
			// EndOfSourceReached state.
			_listeners.Flush();
			_endOfSourceReached = true;
		}

		protected void ResetEndOfSourceReached()
		{
			_endOfSourceReached = false;
		}

		protected void StartTask()
		{
			_readTask = _scheduler.StartPeriodic(Run, ToString());
		}
	}
}