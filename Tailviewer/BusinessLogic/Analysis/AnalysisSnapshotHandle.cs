using System;
using System.Diagnostics.Contracts;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Represents a complete snapshot of an analysis without having the entire snapshot loaded into memory.
	///     Must be used in conjunction with <see cref="AnalysisEngine" /> in order to actually retrieve the snapshot.
	/// </summary>
	public sealed class AnalysisSnapshotHandle
	{
		private readonly AnalysisId _id;
		private readonly string _name;
		private readonly DateTime _creationDate;

		public AnalysisSnapshotHandle(AnalysisId id, string name, DateTime creationDate)
		{
			_id = id;
			_name = name;
			_creationDate = creationDate;
		}

		public AnalysisId Id => _id;
		public string Name => _name;
		public DateTime CreationDate => _creationDate;

		[Pure]
		public static AnalysisSnapshotHandle FromSnapshot(AnalysisSnapshot snapshot)
		{
			return new AnalysisSnapshotHandle(snapshot.Id, null, snapshot.CreationDate);
		}
	}
}