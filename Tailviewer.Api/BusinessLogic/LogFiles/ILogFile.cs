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

		/// <summary>
		/// Copies the original indices of the given log file section into the given array.
		/// The array <paramref name="indices"/> must be at least as big as <see cref="LogFileSection.Count"/>.
		/// </summary>
		/// <param name="section"></param>
		/// <param name="indices"></param>
		void GetOriginalIndices(LogFileSection section, LogLineIndex[] indices);

		[Pure]
		LogLine GetLine(int index);
	}
}