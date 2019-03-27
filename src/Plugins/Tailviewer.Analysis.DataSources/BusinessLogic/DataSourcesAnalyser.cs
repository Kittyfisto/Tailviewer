using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Analysis;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.DataSources.BusinessLogic
{
	public sealed class DataSourcesAnalyser
		: LogAnalyser
	{
		private readonly DataSourcesResult _result;
		private readonly ILogFile _source;
		private readonly HashSet<string> _dataSourceNames;

		public DataSourcesAnalyser(ILogFile source, TimeSpan maximumWaitTime)
		{
			_result = new DataSourcesResult();
			_dataSourceNames = new HashSet<string>();

			_source = source;

			// DO NOT ADD ANYTHING AFTER THIS LINE
			_source.AddListener(this, maximumWaitTime, 1000);
		}

		public override ILogAnalysisResult Result => _result;

		public override Percentage Progress => Percentage.HundredPercent;

		protected override void OnLogFileModifiedInternal(ILogFile logFile, LogFileSection section)
		{
			if (section.IsReset)
			{
				_dataSourceNames.Clear();
				_result.DataSources.Clear();
			}
			else if (section.IsInvalidate)
			{
				// TODO: Implement properly
			}
			else
			{
				var buffer = new LogEntryBuffer(section.Count, LogFileColumns.OriginalDataSourceName);
				logFile.GetEntries(section, buffer);
				foreach (var dataSourceName in buffer.Column(LogFileColumns.OriginalDataSourceName))
				{
					if (_dataSourceNames.Add(dataSourceName))
					{
						_result.DataSources.Add(new DataSource
						{
							Name = dataSourceName
						});
					}
				}
			}
		}

		protected override void DisposeInternal()
		{
			_source.RemoveListener(this);
		}
	}
}
