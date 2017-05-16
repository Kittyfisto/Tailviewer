using System;
using System.Diagnostics.Contracts;
using Metrolib;

namespace Tailviewer.BusinessLogic.LogFiles
{
	public interface ILogFile
		: IDisposable
	{
		DateTime? StartTimestamp { get; }

		DateTime LastModified { get; }

		Size FileSize { get; }

		/// <summary>
		///     Whether or not the datasource exists (is reachable).
		/// </summary>
		bool Exists { get; }

		/// <summary>
		///     Whether or not this log file has reached the end of its data source.
		/// </summary>
		bool EndOfSourceReached { get; }

		int Count { get; }

		/// <summary>
		///     The maximum amount of characters of a single <see cref="LogLine" />.
		/// </summary>
		int MaxCharactersPerLine { get; }

		void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount);
		void RemoveListener(ILogFileListener listener);

		void GetSection(LogFileSection section, LogLine[] dest);
		LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex);

		[Pure]
		LogLine GetLine(int index);
	}
}