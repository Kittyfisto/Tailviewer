using System;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Responsible for encapsulating a <see cref="IDataSourceAnalysisHandle" />.
	///     Whenever the configuration is changed, the previous analysis is stopped
	///     and a new one created.
	/// </summary>
	public sealed class DataSourceAnalyser
		: IDataSourceAnalyser
			, IDataSourceAnalysisListener
			, IDisposable
	{
		private readonly IAnalysisEngine _analysisEngine;
		private readonly LogAnalyserFactoryId _anaylserId;
		private readonly ILogFile _logFile;

		private ILogAnalyserConfiguration _configuration;
		private IDataSourceAnalysisHandle _currentAnalysis;

		public DataSourceAnalyser(ILogFile logFile,
			IAnalysisEngine analysisEngine,
			LogAnalyserFactoryId anaylserId)
		{
			if (logFile == null)
				throw new ArgumentNullException(nameof(logFile));
			if (analysisEngine == null)
				throw new ArgumentNullException(nameof(analysisEngine));

			Id = Guid.NewGuid();
			_logFile = logFile;
			_analysisEngine = analysisEngine;
			_anaylserId = anaylserId;
		}

		public Guid Id { get; }

		public Percentage Progress { get; private set; }

		public ILogAnalysisResult Result { get; private set; }

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

		public void OnProgress(IDataSourceAnalysisHandle handle, Percentage progress)
		{
			if (!ReferenceEquals(handle, _currentAnalysis))
				return; //< It's likely that we've received a callback from a previous analysis that we MOST CERTAINLY need to thrash

			if (Percentage.IsNan(progress))
			{
				Progress = Percentage.HundredPercent;
			}
			else
			{
				Progress = progress;
			}
		}

		public void OnAnalysisResultChanged(IDataSourceAnalysisHandle handle, ILogAnalysisResult result)
		{
			if (!ReferenceEquals(handle, _currentAnalysis))
				return; //< It's likely that we've received a callback from a previous analysis that we MOST CERTAINLY need to thrash

			Result = result;
		}

		public void Dispose()
		{
			_analysisEngine.RemoveAnalysis(_currentAnalysis);
		}

		public DataSourceAnalyserSnapshot CreateSnapshot()
		{
			var configuration = _configuration?.Clone() as ILogAnalyserConfiguration;
			var result = Result?.Clone() as ILogAnalysisResult;
			var progress = Progress;
			return new DataSourceAnalyserSnapshot(Id,
				configuration,
				result,
				progress);
		}

		private void RestartAnalysis()
		{
			if (_currentAnalysis != null)
			{
				_analysisEngine.RemoveAnalysis(_currentAnalysis);
				_currentAnalysis = null;
			}

			if (_configuration != null)
			{
				var configuration = new DataSourceAnalysisConfiguration
				{
					AnalyserId = _anaylserId,
					Configuration = _configuration
				};
				_currentAnalysis = _analysisEngine.CreateAnalysis(_logFile, configuration, this);
				// Now that we've assigned the current analysis, we can actually start it
				// (otherwise there would be the race condition of being notified through the
				// listener interface before we've assigned the new handle, effectively losing
				// the values provided by the callback).
				_currentAnalysis.Start();
			}
		}
	}
}