using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Metrolib;
using log4net;

namespace Tailviewer.BusinessLogic.LogFiles
{
	public abstract class AbstractLogFile
		: ILogFile
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly LogFileListenerCollection _listeners;
		private readonly Task _readTask;
		private bool _isDisposed;
		private volatile bool _endOfSourceReached;

		protected AbstractLogFile()
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_listeners = new LogFileListenerCollection(this);
			_readTask = new Task(Run,
			                     _cancellationTokenSource.Token,
			                     _cancellationTokenSource.Token,
			                     TaskCreationOptions.LongRunning);
		}

		protected LogFileListenerCollection Listeners
		{
			get { return _listeners; }
		}

		public bool IsDisposed
		{
			get { return _isDisposed; }
		}

		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void RemoveListener(ILogFileListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		public virtual void Dispose()
		{
			_cancellationTokenSource.Cancel();
			_isDisposed = true;
		}

		public abstract int MaxCharactersPerLine { get; }

		public abstract bool Exists { get; }

		public virtual bool EndOfSourceReached
		{
			get { return _endOfSourceReached; }
		}

		public abstract DateTime? StartTimestamp { get; }
		public abstract DateTime LastModified { get; }
		public abstract Size FileSize { get; }
		public abstract int Count { get; }

		public abstract void GetSection(LogFileSection section, LogLine[] dest);
		public abstract LogLine GetLine(int index);

		protected abstract void Run(CancellationToken token);

		private void Run(object obj)
		{
			CancellationToken token = _cancellationTokenSource.Token;

			try
			{
				Run(token);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		protected void SetEndOfSourceReached()
		{
			_endOfSourceReached = true;
		}

		protected void ResetEndOfSourceReached()
		{
			_endOfSourceReached = false;
		}

		protected void StartTask()
		{
			if (_readTask.Status == TaskStatus.Created)
			{
				_readTask.Start();
			}
		}
	}
}