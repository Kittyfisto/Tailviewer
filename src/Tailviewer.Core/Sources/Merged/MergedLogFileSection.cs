using System;

namespace Tailviewer.Core.Sources.Merged
{
	internal struct MergedLogFileSection
	{
		public readonly ILogSource LogSource;
		public readonly LogFileSection Section;
		public readonly IReadOnlyLogBuffer Buffer;

		public MergedLogFileSection(ILogSource logSource, LogFileSection section)
		{
			LogSource = logSource;
			Section = section;
			Buffer = null;
		}

		public MergedLogFileSection(ILogSource logSource, LogFileSection section, IReadOnlyLogBuffer buffer)
		{
			LogSource = logSource;
			Section = section;
			Buffer = buffer;
		}

		public override string ToString()
		{
			return String.Format("{0} ({1})", Section, LogSource);
		}
	}
}