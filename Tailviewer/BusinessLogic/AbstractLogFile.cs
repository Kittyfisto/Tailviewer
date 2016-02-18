using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Tailviewer.BusinessLogic
{
	public abstract class AbstractLogFile
		: ILogFile
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly ManualResetEvent _endOfSectionHandle;
		private readonly LogFileListenerCollection _listeners;
		private readonly Task _readTask;

		#region Counts

		private int _debugCount;
		private int _errorCount;
		private int _fatalCount;
		private int _infoCount;
		private int _otherCount;
		private int _warningCount;

		#endregion

		protected AbstractLogFile()
		{
			_endOfSectionHandle = new ManualResetEvent(false);
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

		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void Remove(ILogFileListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		public virtual void Dispose()
		{
			_cancellationTokenSource.Cancel();
		}

		public void Wait()
		{
			Wait(TimeSpan.MaxValue);
		}

		public abstract DateTime? StartTimestamp { get; }
		public abstract DateTime LastModified { get; }
		public abstract Size FileSize { get; }
		public abstract int Count { get; }

		public abstract void GetSection(LogFileSection section, LogLine[] dest);
		public abstract LogLine GetLine(int index);

		#region Counts

		public int FatalCount
		{
			get { return _fatalCount; }
		}

		public int OtherCount
		{
			get { return _otherCount; }
		}

		public int DebugCount
		{
			get { return _debugCount; }
		}

		public int InfoCount
		{
			get { return _infoCount; }
		}

		public int WarningCount
		{
			get { return _warningCount; }
		}

		public int ErrorCount
		{
			get { return _errorCount; }
		}

		#endregion

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

		protected void EndOfSectionReset()
		{
			_endOfSectionHandle.Reset();
		}

		protected void EndOfSectionReached()
		{
			_endOfSectionHandle.Set();
		}

		protected void StartTask()
		{
			if (_readTask.Status == TaskStatus.Created)
			{
				_readTask.Start();
			}
		}

		public bool Wait(TimeSpan waitTime)
		{
			DateTime start = DateTime.Now;
			while (DateTime.Now - start < waitTime)
			{
				if (_endOfSectionHandle.WaitOne(TimeSpan.FromMilliseconds(100)))
					return true;

				if (_readTask.IsFaulted)
					throw _readTask.Exception;
			}

			return false;
		}

		protected void UpdateCounts(LogLine newEntry)
		{
			switch (newEntry.Level)
			{
				case LevelFlags.Debug:
					++_debugCount;
					break;

				case LevelFlags.Info:
					++_infoCount;
					break;

				case LevelFlags.Warning:
					++_warningCount;
					break;

				case LevelFlags.Error:
					++_errorCount;
					break;

				case LevelFlags.Fatal:
					++_fatalCount;
					break;

				default:
					++_otherCount;
					break;
			}
		}
	}
}