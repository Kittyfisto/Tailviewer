using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Represents a snapshot of a previous <see cref="IAnalysis" />.
	///     The snapshot consists of the group's configuration as well as result.
	/// </summary>
	/// <remarks>
	///     TODO: Snapshots should be serializable so they can be stored to disk.
	/// </remarks>
	public sealed class AnalysisSnapshot
		: IAnalysis
		, ISerializableType
	{
		private readonly AnalysisId _id;
		private readonly DataSourceAnalyserSnapshot[] _analysers;
		private readonly Percentage _progress;
		private readonly DateTime _creationDate;

		public AnalysisSnapshot()
		{ }

		public AnalysisSnapshot(Percentage progress, IEnumerable<DataSourceAnalyserSnapshot> analysers)
		{
			if (analysers == null)
				throw new ArgumentNullException(nameof(analysers));

			_id = AnalysisId.CreateNew();
			_creationDate = DateTime.Now;
			_progress = progress;
			_analysers = analysers.ToArray();
		}

		public IEnumerable<IDataSourceAnalyser> Analysers => _analysers;

		public IEnumerable<ILogFile> LogFiles => new ILogFile[0];

		public Percentage Progress => _progress;

		public bool IsFrozen => true;

		public AnalysisId Id => _id;

		public DateTime CreationDate => _creationDate;

		public void Add(DataSourceId id, ILogFile logFile)
		{
			throw new InvalidOperationException("Adding log files to a snapshot is not allowed");
		}

		public void Remove(DataSourceId id, ILogFile logFile)
		{
			throw new InvalidOperationException("Removing log files from a snapshot is not allowed");
		}

		public IDataSourceAnalyser Add(AnalyserPluginId pluginId, ILogAnalyserConfiguration configuration)
		{
			throw new InvalidOperationException("Adding new analysers to a snapshot is not allowed");
		}

		public void Remove(IDataSourceAnalyser analyser)
		{
			throw new InvalidOperationException("Removing analysers from a snapshot is not allowed");
		}

		public bool TryGetAnalyser(AnalyserId analyserId, out IDataSourceAnalyser analyser)
		{
			throw new NotImplementedException();
		}

		public void Serialize(IWriter writer)
		{
			throw new NotImplementedException();
		}

		public void Deserialize(IReader reader)
		{
			throw new NotImplementedException();
		}
	}
}