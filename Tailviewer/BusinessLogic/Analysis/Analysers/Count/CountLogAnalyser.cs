using System;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogTables;

namespace Tailviewer.BusinessLogic.Analysis.Analysers.Count
{
	public sealed class CountLogAnalyser
		: LogAnalyser
	{
		private readonly ILogFile _source;
		private readonly ILogAnalyserConfiguration _configuration;

		public CountLogAnalyser(ILogFile source, ILogAnalyserConfiguration configuration)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));

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