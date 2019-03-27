using System;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Analysis;

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
	{
		private readonly AnalyserTemplate _template;
		private readonly ILogAnalyserEngine _logAnalyserEngine;
		private readonly ILogFile _logFile;

		private ILogAnalyserConfiguration _configuration;
		private IDataSourceAnalysisHandle _currentAnalysis;

		public DataSourceAnalyser(AnalyserTemplate template,
			ILogFile logFile,
			ILogAnalyserEngine logAnalyserEngine)
		{
			if (template == null)
				throw new ArgumentNullException(nameof(template));
			if (logFile == null)
				throw new ArgumentNullException(nameof(logFile));
			if (logAnalyserEngine == null)
				throw new ArgumentNullException(nameof(logAnalyserEngine));

			_template = template;
			_logFile = logFile;
			_logAnalyserEngine = logAnalyserEngine;
			_configuration = template.Configuration;
			RestartAnalysis();
		}

		public AnalyserId Id => _template.Id;

		public LogAnalyserFactoryId LogAnalyserPluginId => _template.LogAnalyserPluginId;

		public DataSourceAnalyserPluginId DataSourceAnalyserPluginId => _template.DataSourceAnalyserPluginId;

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

		public void OnAddLogFile(ILogFile logFile)
		{
			throw new NotImplementedException();
		}

		public void OnRemoveLogFile(ILogFile logFile)
		{
			throw new NotImplementedException();
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
			_logAnalyserEngine.RemoveAnalysis(_currentAnalysis);
		}

		public DataSourceAnalyserSnapshot CreateSnapshot()
		{
			var configuration = _configuration?.Clone() as ILogAnalyserConfiguration;
			var result = Result?.Clone() as ILogAnalysisResult;
			var progress = Progress;
			return new DataSourceAnalyserSnapshot(Id,
				LogAnalyserPluginId,
				configuration,
				result,
				progress);
		}

		private void RestartAnalysis()
		{
			if (_currentAnalysis != null)
			{
				_logAnalyserEngine.RemoveAnalysis(_currentAnalysis);
				_currentAnalysis = null;
			}

			var configuration = new DataSourceAnalysisConfiguration
			{
				FactoryId = _template.LogAnalyserPluginId,
				Configuration = _configuration
			};
			_currentAnalysis = _logAnalyserEngine.CreateAnalysis(_logFile, configuration, this);
			// Now that we've assigned the current analysis, we can actually start it
			// (otherwise there would be the race condition of being notified through the
			// listener interface before we've assigned the new handle, effectively losing
			// the values provided by the callback).
			_currentAnalysis.Start();
		}
	}
}