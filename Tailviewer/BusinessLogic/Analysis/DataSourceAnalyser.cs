using System;
using Tailviewer.BusinessLogic.Analysis.Analysers;
using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Responsible for encapsulating a <see cref="IDataSourceAnalysis" />.
	///     Whenever the configuration is changed, the previous analysis is stopped
	///     and a new one created.
	/// </summary>
	public sealed class DataSourceAnalyser
		: IDataSourceAnalyser
			, IDisposable
	{
		private readonly IAnalysisEngine _analysisEngine;
		private readonly IDataSource _dataSource;
		private readonly LogAnalyserFactoryId _anaylserId;
		private IDataSourceAnalysis _analysis;

		private ILogAnalyserConfiguration _configuration;

		public DataSourceAnalyser(IDataSource dataSource,
			IAnalysisEngine analysisEngine,
			LogAnalyserFactoryId anaylserId)
		{
			if (dataSource == null)
				throw new ArgumentNullException(nameof(dataSource));
			if (analysisEngine == null)
				throw new ArgumentNullException(nameof(analysisEngine));

			Id = Guid.NewGuid();
			_dataSource = dataSource;
			_analysisEngine = analysisEngine;
			_anaylserId = anaylserId;
		}

		public Guid Id { get; }

		public ILogAnalysisResult Result => _analysis?.Result;

		public bool IsFrozen => false;

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

		public DataSourceAnalyserSnapshot CreateSnapshot()
		{
			var configuration = _configuration?.Clone() as ILogAnalyserConfiguration;
			var result = Result?.Clone() as ILogAnalysisResult;
			return new DataSourceAnalyserSnapshot(Id,
				configuration,
				result);
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
					AnalyserId = _anaylserId,
					Configuration = _configuration
				};
				_analysis = _analysisEngine.CreateAnalysis(_dataSource, configuration);
			}
		}
	}
}