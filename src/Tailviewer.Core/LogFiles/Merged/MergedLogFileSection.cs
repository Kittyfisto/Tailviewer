using System;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles.Merged
{
	internal struct MergedLogFileSection
	{
		public readonly ILogFile LogFile;
		public readonly LogFileSection Section;
		public readonly IReadOnlyLogEntries Entries;

		public MergedLogFileSection(ILogFile logFile, LogFileSection section)
		{
			LogFile = logFile;
			Section = section;
			Entries = null;
		}

		public MergedLogFileSection(ILogFile logFile, LogFileSection section, IReadOnlyLogEntries entries)
		{
			LogFile = logFile;
			Section = section;
			Entries = entries;
		}

		public override string ToString()
		{
			return String.Format("{0} ({1})", Section, LogFile);
		}
	}
}