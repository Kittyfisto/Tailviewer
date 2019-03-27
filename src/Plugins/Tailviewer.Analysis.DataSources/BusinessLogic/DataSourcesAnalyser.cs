using System.Collections.Generic;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Analysis.DataSources.BusinessLogic
{
	public sealed class DataSourcesAnalyser
		: IDataSourceAnalyser
	{
		private readonly AnalyserId _id;
		private readonly DataSourcesResult _result;
		private readonly Dictionary<ILogFile, DataSource> _logFiles;

		public DataSourcesAnalyser(AnalyserId id)
		{
			_id = id;
			_result = new DataSourcesResult();
			_logFiles = new Dictionary<ILogFile, DataSource>();
		}

		public ILogAnalysisResult Result => _result;

		public bool IsFrozen => false;

		public ILogAnalyserConfiguration Configuration { get; set; }

		public void OnLogFileAdded(ILogFile logFile)
		{
			var dataSource = new DataSource
			{
				Name = logFile.GetValue(LogFileProperties.Name)
			};
			_logFiles.Add(logFile, dataSource);
			_result.DataSources.Add(dataSource);
		}

		public void OnLogFileRemoved(ILogFile logFile)
		{
			if (_logFiles.TryGetValue(logFile, out var dataSource))
			{
				_logFiles.Remove(logFile);
				_result.DataSources.Remove(dataSource);
			}
		}

		public AnalyserId Id => _id;

		public AnalyserPluginId AnalyserPluginId => DataSourcesAnalyserPlugin.Id;

		public Percentage Progress => Percentage.HundredPercent;

		#region Implementation of IDisposable

		public void Dispose()
		{}

		#endregion
	}
}
