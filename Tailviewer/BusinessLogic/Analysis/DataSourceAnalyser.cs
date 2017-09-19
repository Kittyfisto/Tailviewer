using System;
using Tailviewer.BusinessLogic.Analysis.Analysers;
using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Represents a continuous analysis of a data source given a possibly
	///     changing configuration.
	/// </summary>
	public sealed class DataSourceAnalyser
		: IDataSourceAnalyser
			, IDisposable
	{
		private readonly IAnalysisEngine _analysisEngine;
		private readonly IDataSource _dataSource;
		private readonly Type _logAnalyserType;
		private IDataSourceAnalysis _analysis;

		private ILogAnalyserConfiguration _configuration;

		public DataSourceAnalyser(IDataSource dataSource,
			IAnalysisEngine analysisEngine,
			Type logAnalyserType)
		{
			if (dataSource == null)
				throw new ArgumentNullException(nameof(dataSource));
			if (analysisEngine == null)
				throw new ArgumentNullException(nameof(analysisEngine));
			if (logAnalyserType == null)
				throw new ArgumentNullException(nameof(logAnalyserType));

			_dataSource = dataSource;
			_analysisEngine = analysisEngine;
			_logAnalyserType = logAnalyserType;
		}

		public ILogAnalysisResult Result => _analysis?.Result;

		public ILogAnalyserConfiguration Configuration
		{
			get { return _configuration; }
			set
			{
				if (Equals(_configuration, value))
					return;

				_configuration = value;
				RestartAnalysis();
			}
		}

		public void Dispose()
		{
			_analysisEngine.RemoveAnalysis(_analysis);
		}

		private void RestartAnalysis()
		{
			if (_analysis != null)
			{
				_analysisEngine.RemoveAnalysis(_analysis);
				_analysis = null;
			}

			if (_configuration != null)
			{
				var configuration = new DataSourceAnalysisConfiguration
				{
					LogAnalyserType = _logAnalyserType,
					Configuration = _configuration
				};
				_analysis = _analysisEngine.CreateAnalysis(_dataSource, configuration);
			}
		}
	}
}