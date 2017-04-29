using System;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.BusinessLogic.Analysers
{
	public sealed class CounterLogAnalyser
		: LogAnalyser
	{
		public CounterLogAnalyser(ILogFile source, TimeSpan maximumWaitTime, int maximumLineCount)
		{
		}

		protected override void OnLogFileModifiedInternal(ILogFile logFile, LogFileSection section)
		{
			throw new NotImplementedException();
		}

		protected override void OnLogTableModifiedInternal(ILogTable logTable, LogTableModification modification)
		{
			throw new NotImplementedException();
		}

		protected override void DisposeInternal()
		{
			throw new NotImplementedException();
		}
	}
}