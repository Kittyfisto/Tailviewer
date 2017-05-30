using System;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.BusinessLogic.Analysis.Analysers
{
	public sealed class QuickInfoLogAnalyser
		: LogAnalyser
	{
		public QuickInfoLogAnalyser(ILogFile source, ILogAnalyserConfiguration configuration)
		{
			
		}

		public override ILogAnalysisResult Result => null;

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