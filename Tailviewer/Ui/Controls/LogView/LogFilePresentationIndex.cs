using System;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Ui.Controls.LogView
{
	/// <summary>
	///     Indexes the contents of an <see cref="ILogFile" /> in a way that is useful for its presentation.
	/// </summary>
	/// <remarks>
	/// Keeps track of the number of lines in a 
	/// </remarks>
	public sealed class LogFilePresentationIndex
		: ILogFileListener
	{
		private readonly int _maximumLineCount;
		private readonly TimeSpan _maximumWaitTime;
		private ILogFile _logFile;

		public LogFilePresentationIndex()
			: this(TimeSpan.FromMilliseconds(value: 100))
		{
		}

		public LogFilePresentationIndex(TimeSpan maximumWaitTime)
		{
			_maximumWaitTime = maximumWaitTime;
			_maximumLineCount = 1000;
		}

		public ILogFile LogFile
		{
			get { return _logFile; }
			set
			{
				_logFile?.RemoveListener(this);
				_logFile = value;
				_logFile?.AddListener(this, _maximumWaitTime, _maximumLineCount);
			}
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
		}
	}
}