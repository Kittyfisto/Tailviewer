using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.Analysis.Analysers;
using Tailviewer.BusinessLogic.LogFiles;

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
		private readonly DataSourceAnalyserSnapshot[] _analysers;

		public AnalyserGroupSnapshot(IEnumerable<DataSourceAnalyserSnapshot> analysers)
		{
			if (analysers == null)
				throw new ArgumentNullException(nameof(analysers));

			_analysers = analysers.ToArray();
		}

		public IEnumerable<IDataSourceAnalyser> Analysers => _analysers;

		public IEnumerable<ILogFile> LogFiles => new ILogFile[0];

		public bool IsFrozen => true;
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