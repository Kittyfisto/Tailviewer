using System;
using System.Diagnostics.Contracts;

namespace Tailviewer.BusinessLogic.LogFiles
{
	public interface ILogFile
		: IDisposable
	{
		DateTime? StartTimestamp { get; }

		DateTime LastModified { get; }

		Size FileSize { get; }

		/// <summary>
		/// Whether or not the datasource exists (is reachable).
		/// </summary>
		bool Exists { get; }

		void Wait();
		void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount);
		void Remove(ILogFileListener listener);

		void GetSection(LogFileSection section, LogLine[] dest);
		int Count { get; }

		[Pure]
		LogLine GetLine(int index);
	}
}