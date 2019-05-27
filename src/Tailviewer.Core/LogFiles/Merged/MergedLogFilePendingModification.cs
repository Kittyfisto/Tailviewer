using System;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles.Merged
{
	internal struct MergedLogFilePendingModification
	{
		public readonly ILogFile LogFile;
		public readonly LogFileSection Section;

		public MergedLogFilePendingModification(ILogFile logFile, LogFileSection section)
		{
			LogFile = logFile;
			Section = section;
		}

		public override string ToString()
		{
			return String.Format("{0} ({1})", Section, LogFile);
		}
	}
}