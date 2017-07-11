using System;
using System.Collections.Generic;
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
		int OriginalCount { get; }

		/// <summary>
		///     The maximum amount of characters of a single <see cref="LogLine" />.
		/// </summary>
		int MaxCharactersPerLine { get; }

		void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount);
		void RemoveListener(ILogFileListener listener);

		void GetSection(LogFileSection section, LogLine[] dest);


		[Pure]
		LogLine GetLine(int index);

		#region Indices

		/// <summary>
		/// 
		/// </summary>
		/// <param name="originalLineIndex"></param>
		/// <returns></returns>
		LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex);

		/// <summary>
		///     Returns the original log line index for the given index.
		/// </summary>
		/// <remarks>
		///     Returns the same index as given unless this log file filters or reorders
		///     log lines.
		/// </remarks>
		/// <param name="index"></param>
		/// <returns></returns>
		LogLineIndex GetOriginalIndexFrom(LogLineIndex index);

		/// <summary>
		///     Copies the original indices of the given log file section into the given array.
		///     The array <paramref name="originalIndices" /> must be at least as big as <see cref="LogFileSection.Count" />.
		/// </summary>
		/// <param name="section"></param>
		/// <param name="originalIndices"></param>
		void GetOriginalIndicesFrom(LogFileSection section, LogLineIndex[] originalIndices);

		/// <summary>
		///     Copies the original indices of the given log list of indices
		/// </summary>
		/// <param name="indices"></param>
		/// <param name="originalIndices"></param>
		void GetOriginalIndicesFrom(IReadOnlyList<LogLineIndex> indices, LogLineIndex[] originalIndices);

		#endregion
	}
}