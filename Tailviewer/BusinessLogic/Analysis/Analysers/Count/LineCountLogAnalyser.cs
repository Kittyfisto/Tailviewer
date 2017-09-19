using System;
using System.Threading;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogTables;

namespace Tailviewer.BusinessLogic.Analysis.Analysers.Count
{
	public sealed class LineCountLogAnalyser
		: LogAnalyser
	{
		private readonly ILogFile _source;
		private readonly ILogAnalyserConfiguration _configuration;
		private readonly ITaskScheduler _scheduler;

		public LineCountLogAnalyser(ITaskScheduler scheduler, ILogFile source, ILogAnalyserConfiguration configuration)
		{
			if (scheduler == null)
				throw new ArgumentNullException(nameof(scheduler));
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			_scheduler = scheduler;
			_source = source;
			_configuration = configuration;
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