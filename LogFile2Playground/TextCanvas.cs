using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Tailviewer.BusinessLogic.LogFiles;

namespace LogFile2Playground
{
	public sealed class TextCanvas
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private ILogFile2 _logFile;

		private CancellationTokenSource _cancellationTokenSource;
		private Task<LogLineResponse> _getLinesTask;

		public ILogFile2 LogFile
		{
			get { return _logFile; }
			set
			{
				if (ReferenceEquals(value, _logFile))
					return;

				_logFile = value;
				CancelGetLinesTask();
				if (value != null)
				{
					RequestVisibleSection();
				}
			}
		}

		private void RequestVisibleSection()
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_getLinesTask = _logFile.RequestAsync(VisibleSection, _cancellationTokenSource.Token);
			_getLinesTask.ContinueWith(LogException);
		}

		private void LogException(Task<LogLineResponse> task)
		{
			var exception = task.Exception;
			if (exception != null)
			{
				Log.DebugFormat("Caught exception: {0}", exception);
			}
		}

		private void CancelGetLinesTask()
		{
			if (_cancellationTokenSource != null)
			{
				_cancellationTokenSource.Cancel();
				_getLinesTask = null;
			}
		}

		public LogFileSection VisibleSection { get; set; }

		public void Update()
		{
			if (_getLinesTask?.IsCompleted == true)
			{
				var result = _getLinesTask.Result;
				Draw(result.Lines, result.ActualSection);
			}
			else
			{
				// Still waiting for data to arrive...
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="resultLines"></param>
		/// <param name="section">The section of the log file which is to be drawn</param>
		private void Draw(LogLine[] resultLines, LogFileSection section)
		{
			throw new System.NotImplementedException();
		}
	}
}