using System;
using System.Threading;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core;
using Tailviewer.Core.LogTables;

namespace Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo
{
	public sealed class QuickInfoAnalyser
		: LogAnalyser
	{
		public QuickInfoAnalyser(ITaskScheduler scheduler,
			ILogFile source,
			TimeSpan timeSpan,
			QuickInfoAnalyserConfiguration configuration)
		{
		}

		public override ILogAnalysisResult Result => null;

		public override Percentage Progress
		{
			get { throw new NotImplementedException(); }
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