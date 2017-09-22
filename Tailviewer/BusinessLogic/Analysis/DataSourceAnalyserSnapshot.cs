using System;
using Tailviewer.BusinessLogic.Analysis.Analysers;
using Tailviewer.Core;

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

		public DataSourceAnalyserSnapshot(Guid id, ILogAnalyserConfiguration configuration, ILogAnalysisResult result, Percentage progress)
		{
			if (id == null)
				throw new ArgumentNullException(nameof(id));

			Id = id;
			_configuration = configuration;
			_result = result;
			_progress = progress;
		}

		public Guid Id { get; }

		public Percentage Progress => _progress;

		public ILogAnalysisResult Result => _result;

		public bool IsFrozen => true;

		public ILogAnalyserConfiguration Configuration
		{
			get { return _configuration; }
			set { throw new InvalidOperationException("Changing the configuration of a snapshot is not allowed"); }
		}
	}
}