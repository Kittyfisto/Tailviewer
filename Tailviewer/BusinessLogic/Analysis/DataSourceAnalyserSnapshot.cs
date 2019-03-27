using System;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Represents a snapshot of a previous <see cref="IDataSourceAnalyser" />'s
	///     configuration and result.
	/// </summary>
	/// <remarks>
	///     TODO: Snapshots should be serializable so they can be stored to disk.
	/// </remarks>
	public sealed class DataSourceAnalyserSnapshot
		: IDataSourceAnalyser
	{
		private readonly ILogAnalyserConfiguration _configuration;
		private readonly Percentage _progress;
		private readonly ILogAnalysisResult _result;

		public DataSourceAnalyserSnapshot(AnalyserId id,
			LogAnalyserFactoryId factoryId,
			ILogAnalyserConfiguration configuration,
			ILogAnalysisResult result,
			Percentage progress)
		{
			if (id == AnalyserId.Empty)
				throw new ArgumentException(nameof(id));
			if (factoryId ==  LogAnalyserFactoryId.Empty)
				throw new ArgumentException(nameof(factoryId));

			Id = id;
			LogAnalyserPluginId = factoryId;
			_configuration = configuration;
			_result = result;
			_progress = progress;
		}

		public AnalyserId Id { get; }

		public LogAnalyserFactoryId LogAnalyserPluginId { get; }

		public DataSourceAnalyserPluginId DataSourceAnalyserPluginId => DataSourceAnalyserPluginId.Empty;

		public Percentage Progress => _progress;

		public ILogAnalysisResult Result => _result;

		public bool IsFrozen => true;

		public ILogAnalyserConfiguration Configuration
		{
			get { return _configuration; }
			set { throw new InvalidOperationException("Changing the configuration of a snapshot is not allowed"); }
		}

		public void OnAddLogFile(ILogFile logFile)
		{
			throw new InvalidOperationException("Adding a log file to a snapshot is not allowed");
		}

		public void OnRemoveLogFile(ILogFile logFile)
		{
			throw new InvalidOperationException("Removing a log file from a snapshot is not allowed");
		}

		public void Dispose()
		{
		}
	}
}