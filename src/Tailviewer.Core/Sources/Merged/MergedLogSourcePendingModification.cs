using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Tailviewer.Core.Sources.Merged
{
	internal readonly struct MergedLogSourcePendingModification
	{
		public readonly ILogSource LogSource;
		public readonly LogSourceModification Modification;

		public MergedLogSourcePendingModification(ILogSource logSource, LogSourceModification modification)
		{
			LogSource = logSource;
			Modification = modification;
		}

		public override string ToString()
		{
			return $"{Modification} ({LogSource})";
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
				var section = modification.Modification;
				if (section.IsReset())
				{
					modifications.RemoveAll(x => x.LogSource == logFile);
				}

				modifications.Add(modification);
			}

			return modifications;
		}
	}
}