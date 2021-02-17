using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Tailviewer.Core.Sources.Merged
{
	internal struct MergedLogSourcePendingModification
	{
		public readonly ILogSource LogSource;
		public readonly LogFileSection Section;

		public MergedLogSourcePendingModification(ILogSource logSource, LogFileSection section)
		{
			LogSource = logSource;
			Section = section;
		}

		public override string ToString()
		{
			return $"{Section} ({LogSource})";
		}

		/// <summary>
		///     Returns an optimized list of changes which causes identical behaviour as the input list, but which may
		///     execute faster.
		/// </summary>
		/// <remarks>
		///     This method looks at the list of modifications and identifies resets. All modifications to the SAME source
		///     prior to this reset will be ignored and not forwarded.
		/// </remarks>
		/// <remarks>
		///     TODO: Implement similar algorithm for invalidations
		///     All modifications concerning a region which is invalidated later on should be changed so these modifications no
		///     longer concern invalidated regions. All modifications which are fully inside the invalidated interval can even
		///     be removed.
		/// </remarks>
		/// <param name="pendingModifications"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyList<MergedLogSourcePendingModification> Optimize(
			IEnumerable<MergedLogSourcePendingModification> pendingModifications)
		{
			var modifications = new List<MergedLogSourcePendingModification>();

			foreach(var modification in pendingModifications)
			{
				var logFile = modification.LogSource;
				var section = modification.Section;
				if (section.IsReset)
				{
					modifications.RemoveAll(x => x.LogSource == logFile);
				}

				modifications.Add(modification);
			}

			return modifications;
		}
	}
}