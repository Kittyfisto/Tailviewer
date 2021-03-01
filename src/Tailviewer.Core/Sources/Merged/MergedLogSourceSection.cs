using System;

namespace Tailviewer.Core.Sources.Merged
{
	internal readonly struct MergedLogSourceSection
	{
		public readonly ILogSource LogSource;
		public readonly LogSourceModification Modification;
		public readonly IReadOnlyLogBuffer Buffer;

		public MergedLogSourceSection(ILogSource logSource, LogSourceModification modification)
		{
			LogSource = logSource;
			Modification = modification;
			Buffer = null;
		}

		public MergedLogSourceSection(ILogSource logSource, LogSourceModification modification, IReadOnlyLogBuffer buffer)
		{
			LogSource = logSource;
			Modification = modification;
			Buffer = buffer;
		}

		public override string ToString()
		{
			return String.Format("{0} ({1})", Modification, LogSource);
		}
	}
}