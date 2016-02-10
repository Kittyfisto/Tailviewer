using System;
using System.Diagnostics.Contracts;

namespace Tailviewer.BusinessLogic
{
	public interface ILogFile
		: IDisposable
	{
		DateTime? StartTimestamp { get; }

		DateTime LastModified { get; }

		Size FileSize { get; }

		int Count { get; }
		int OtherCount { get; }
		int DebugCount { get; }
		int InfoCount { get; }
		int WarningCount { get; }
		int ErrorCount { get; }
		int FatalCount { get; }

		void Wait();
		void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount);
		void Remove(ILogFileListener listener);

		void GetSection(LogFileSection section, LogLine[] dest);

		[Pure]
		LogLine GetLine(int index);
	}
}