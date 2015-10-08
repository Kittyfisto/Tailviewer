using System;
using System.Diagnostics.Contracts;

namespace SharpTail.BusinessLogic
{
	public interface ILogFile
		: IDisposable
	{
		void Start();
		void Wait();

		int Count { get; }

		void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount);
		void Remove(ILogFileListener listener);

		void GetSection(LogFileSection section, string[] dest);

		[Pure]
		string GetEntry(int index);
	}
}