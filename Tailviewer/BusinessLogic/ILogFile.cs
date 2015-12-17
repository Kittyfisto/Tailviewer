using System;
using System.Diagnostics.Contracts;

namespace Tailviewer.BusinessLogic
{
	public interface ILogFile
		: IDisposable
	{
		void Wait();

		int Count { get; }

		void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount);
		void Remove(ILogFileListener listener);

		void GetSection(LogFileSection section, LogLine[] dest);

		[Pure]
		LogLine GetEntry(int index);
	}
}