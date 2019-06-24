using System;
using System.Linq;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Analysis;
using Tailviewer.Core.Filters;
using Tailviewer.Core.Settings;

namespace Tailviewer.Analysis.Count.BusinessLogic
{
	public sealed class LogEntryCountAnalyser
		: AbstractLogAnalyser
	{
		private const int MaximumLineCount = 10000;

		private LogEntryCountResult _result;
		private readonly ILogFile _logFile;
		private readonly bool _ownsLogFile;

		public LogEntryCountAnalyser(IServiceContainer services, ILogFile logFile, TimeSpan maximumWaitTime, LogEntryCountAnalyserConfiguration configuration)
			: base(services)
		{
			if (services == null)
				throw new ArgumentNullException(nameof(services));
			if (logFile == null)
				throw new ArgumentNullException(nameof(logFile));
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			var filter = CreateFilter(configuration.QuickFilters);
			if (filter != null)
			{
				_logFile = services.CreateFilteredLogFile(maximumWaitTime, logFile, filter);
				_ownsLogFile = true;
			}
			else
			{
				_logFile = logFile;
			}

			// EVERYTHING MUST BE INITIALIZED AFTER THIS LINE AS OnLogFileModifiedInternal
			// WILL BE CALLED NEXT
			_logFile.AddListener(this, maximumWaitTime, MaximumLineCount);
		}

		private ILogEntryFilter CreateFilter(QuickFilters configurationQuickFilters)
		{
			return Filter.Create(configurationQuickFilters.Select(x => x.CreateFilter()));
		}

		public override ILogAnalysisResult Result => _result;

		public override Percentage Progress => Percentage.FromPercent((float)_logFile.Progress * 100);

		protected override void OnLogFileModifiedInternal(ILogFile logFile, LogFileSection section)
		{
			_result = new LogEntryCountResult
			{
				Count = _logFile.Count
			};
		}

		protected override void DisposeInternal()
		{
			_logFile.RemoveListener(this);
			if (_ownsLogFile)
			{
				_logFile.Dispose();
			}
		}
	}
}