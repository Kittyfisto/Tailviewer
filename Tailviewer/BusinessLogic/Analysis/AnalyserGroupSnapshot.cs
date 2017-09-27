using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Represents a snapshot of a previous <see cref="IAnalyserGroup" />.
	///     The snapshot consists of the group's configuration as well as result.
	/// </summary>
	/// <remarks>
	///     TODO: Snapshots should be serializable so they can be stored to disk.
	/// </remarks>
	public sealed class AnalyserGroupSnapshot
		: IAnalyserGroup
	{
		private readonly AnalysisId _id;
		private readonly DataSourceAnalyserSnapshot[] _analysers;
		private readonly Percentage _progress;

		public AnalyserGroupSnapshot(Percentage progress, IEnumerable<DataSourceAnalyserSnapshot> analysers)
		{
			if (analysers == null)
				throw new ArgumentNullException(nameof(analysers));

			_id = AnalysisId.CreateNew();
			_progress = progress;
			_analysers = analysers.ToArray();
		}

		public IEnumerable<IDataSourceAnalyser> Analysers => _analysers;

		public IEnumerable<ILogFile> LogFiles => new ILogFile[0];

		public Percentage Progress
		{
			get { return _progress; }
		}

		public bool IsFrozen => true;

		public AnalysisId Id => _id;

		public void Add(ILogFile logFile)
		{
			throw new InvalidOperationException("Adding log files to a snapshot is not allowed");
		}

		public void Remove(ILogFile logFile)
		{
			throw new InvalidOperationException("Removing log files from a snapshot is not allowed");
		}

		public IDataSourceAnalyser Add(LogAnalyserFactoryId analyserId, ILogAnalyserConfiguration configuration)
		{
			throw new InvalidOperationException("Adding new analysers to a snapshot is not allowed");
		}

		public void Remove(IDataSourceAnalyser analyser)
		{
			throw new InvalidOperationException("Removing analysers from a snapshot is not allowed");
		}
	}
}