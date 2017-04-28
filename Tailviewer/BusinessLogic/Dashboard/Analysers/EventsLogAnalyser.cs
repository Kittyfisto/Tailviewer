using System;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.BusinessLogic.Dashboard.Analysers
{
	public sealed class EventsLogAnalyser
		: LogAnalyser
	{


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