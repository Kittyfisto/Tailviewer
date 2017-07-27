using System;
using System.Collections.Generic;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     An abstract <see cref="ILogFile" /> implementation which allows a subclass to modify
	///     the <see cref="LogLine" />s of the proxied log file.
	/// </summary>
	/// <remarks>
	///     This class may be used in plugin developmen when <see cref="TextLogFile" /> isn't enough on its own, for example
	///     because timestamps or levels aren't properly detected. A subclass may then use <see cref="TextLogFile" /> and
	///     simply translate the lines as it sees fit.
	/// </remarks>
	public abstract class ProxyLogFile
		: ILogFile
	{
		private readonly ILogFile _actualLogFile;

		/// <summary>
		///     Initializes this log file.
		/// </summary>
		/// <remarks>
		///     Takes ownership of the given <paramref name="actualLogFile" /> and disposes of it
		///     when this log file is disposed of.
		/// </remarks>
		/// <param name="actualLogFile"></param>
		protected ProxyLogFile(ILogFile actualLogFile)
		{
			if (actualLogFile == null)
				throw new ArgumentNullException(nameof(actualLogFile));

			_actualLogFile = actualLogFile;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_actualLogFile.Dispose();
		}

		/// <inheritdoc />
		public DateTime? StartTimestamp => _actualLogFile.StartTimestamp;

		/// <inheritdoc />
		public DateTime LastModified => _actualLogFile.LastModified;

		/// <inheritdoc />
		public Size Size => _actualLogFile.Size;

		/// <inheritdoc />
		public bool Exists => _actualLogFile.Exists;

		/// <inheritdoc />
		public bool EndOfSourceReached => _actualLogFile.EndOfSourceReached;

		/// <inheritdoc />
		public int Count => _actualLogFile.Count;

		/// <inheritdoc />
		public int OriginalCount => _actualLogFile.OriginalCount;

		/// <inheritdoc />
		public int MaxCharactersPerLine => _actualLogFile.MaxCharactersPerLine;

		/// <inheritdoc />
		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_actualLogFile.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		/// <inheritdoc />
		public void RemoveListener(ILogFileListener listener)
		{
			_actualLogFile.RemoveListener(listener);
		}

		/// <inheritdoc />
		public void GetSection(LogFileSection section, LogLine[] dest)
		{
			_actualLogFile.GetSection(section, dest);
			for (var i = 0; i < section.Count; ++i)
				dest[i] = Translate(dest[i]);
		}

		/// <inheritdoc />
		public LogLine GetLine(int index)
		{
			var line = _actualLogFile.GetLine(index);
			line = Translate(line);
			return line;
		}

		/// <inheritdoc />
		public double Progress => _actualLogFile.Progress;

		/// <inheritdoc />
		public LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			return _actualLogFile.GetLogLineIndexOfOriginalLineIndex(originalLineIndex);
		}

		/// <inheritdoc />
		public LogLineIndex GetOriginalIndexFrom(LogLineIndex index)
		{
			return _actualLogFile.GetOriginalIndexFrom(index);
		}

		/// <inheritdoc />
		public void GetOriginalIndicesFrom(LogFileSection section, LogLineIndex[] originalIndices)
		{
			_actualLogFile.GetOriginalIndicesFrom(section, originalIndices);
		}

		/// <inheritdoc />
		public void GetOriginalIndicesFrom(IReadOnlyList<LogLineIndex> indices, LogLineIndex[] originalIndices)
		{
			_actualLogFile.GetOriginalIndicesFrom(indices, originalIndices);
		}

		/// <summary>
		///     Translates the given line into whatever form the subclass deems useful.
		/// </summary>
		/// <param name="line"></param>
		/// <returns>The new line that shall be exposed by this log file</returns>
		protected abstract LogLine Translate(LogLine line);
	}
}