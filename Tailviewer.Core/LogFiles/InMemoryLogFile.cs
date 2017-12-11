using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     TODO: Use this implementation is tests, where applicable (should reduce number of mocks...).
	/// </summary>
	public sealed class InMemoryLogFile
		: ILogFile
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly List<LogLine> _lines;
		private readonly LogFileListenerCollection _listeners;

		private readonly object _syncRoot;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		public InMemoryLogFile()
		{
			_syncRoot = new object();
			_lines = new List<LogLine>();
			_listeners = new LogFileListenerCollection(this);
		}

		/// <inheritdoc />
		public void Dispose()
		{
		}

		/// <inheritdoc />
		public DateTime? StartTimestamp { get; private set; }

		/// <inheritdoc />
		public DateTime LastModified { get; private set; }

		/// <inheritdoc />
		public DateTime Created => DateTime.MinValue;

		/// <inheritdoc />
		public Size Size { get; set; }

		/// <inheritdoc />
		public ErrorFlags Error => ErrorFlags.None;

		/// <inheritdoc />
		public bool EndOfSourceReached => true;

		/// <inheritdoc />
		public int Count => _lines.Count;

		/// <inheritdoc />
		public int OriginalCount => Count;

		/// <inheritdoc />
		public int MaxCharactersPerLine { get; private set; }

		/// <inheritdoc />
		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		/// <inheritdoc />
		public void RemoveListener(ILogFileListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		/// <inheritdoc />
		public void GetColumn<T>(LogFileSection section, ILogFileColumn<T> column, T[] buffer)
		{
			if (column == LogFileColumns.TimeStamp)
			{
				GetTimestamps(section, (DateTime?[])(object)buffer);
			}
			else if (column == LogFileColumns.RawContent)
			{
				GetRawContent(section, (string[]) (object) buffer);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <inheritdoc />
		public void GetColumn<T>(IReadOnlyList<LogLineIndex> indices, ILogFileColumn<T> column, T[] buffer)
		{
			if (column == LogFileColumns.TimeStamp)
			{
				GetTimestamps(indices, (DateTime?[]) (object) buffer);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		private void GetRawContent(LogFileSection section, string[] buffer)
		{
			lock (_lines)
			{
				for (int i = 0; i < section.Count; ++i)
				{
					var index = section.Index + i;
					buffer[i] = GetRawContent(index);
				}
			}
		}

		private void GetTimestamps(LogFileSection section, DateTime?[] buffer)
		{
			lock (_lines)
			{
				for (int i = 0; i < section.Count; ++i)
				{
					var index = section.Index + i;
					buffer[i] = GetTimestamp(index);
				}
			}
		}

		private void GetTimestamps(IReadOnlyList<LogLineIndex> indices, DateTime?[] buffer)
		{
			lock (_lines)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					buffer[i] = GetTimestamp(index);
				}
			}
		}

		private string GetRawContent(LogLineIndex index)
		{
			if (index < 0 || index >= _lines.Count)
			{
				if (Log.IsDebugEnabled)
					Log.DebugFormat("{0}: Row {1} doesn't exist", this, index);

				return null;
			}

			return _lines[index.Value].Message;
		}

		private DateTime? GetTimestamp(LogLineIndex index)
		{
			if (index < 0 || index >= _lines.Count)
			{
				if (Log.IsDebugEnabled)
					Log.DebugFormat("{0}: Row {1} doesn't exist", this, index);

				return null;
			}

			return _lines[index.Value].Timestamp;
		}

		/// <inheritdoc />
		public void GetSection(LogFileSection section, LogLine[] dest)
		{
			lock (_lines)
			{
				_lines.CopyTo((int) section.Index, dest, 0, section.Count);
			}
		}

		/// <inheritdoc />
		public LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			lock (_lines)
			{
				if (originalLineIndex >= _lines.Count)
				{
					return LogLineIndex.Invalid;
				}

				return originalLineIndex;
			}
		}

		/// <inheritdoc />
		public LogLineIndex GetOriginalIndexFrom(LogLineIndex index)
		{
			lock (_lines)
			{
				if (index >= _lines.Count)
				{
					return LogLineIndex.Invalid;
				}

				return index;
			}
		}

		/// <inheritdoc />
		public void GetOriginalIndicesFrom(LogFileSection section, LogLineIndex[] originalIndices)
		{
			if (originalIndices == null)
				throw new ArgumentNullException(nameof(originalIndices));
			if (originalIndices.Length < section.Count)
				throw new ArgumentOutOfRangeException(nameof(originalIndices));

			lock (_lines)
			{
				for (int i = 0; i < section.Count; ++i)
				{
					var index = section.Index + i;
					if (index >= _lines.Count)
						originalIndices[i] = LogLineIndex.Invalid;
					else
						originalIndices[i] = index;
				}
			}
		}

		/// <inheritdoc />
		public void GetOriginalIndicesFrom(IReadOnlyList<LogLineIndex> indices, LogLineIndex[] originalIndices)
		{
			if (indices == null)
				throw new ArgumentNullException(nameof(indices));
			if (originalIndices == null)
				throw new ArgumentNullException(nameof(originalIndices));
			if (indices.Count > originalIndices.Length)
				throw new ArgumentOutOfRangeException();

			for (int i = 0; i < indices.Count; ++i)
			{
				originalIndices[i] = indices[i];
			}
		}

		/// <inheritdoc />
		public LogLine GetLine(int index)
		{
			lock (_lines)
			{
				return _lines[index];
			}
		}

		/// <inheritdoc />
		public double Progress => 1;

		/// <summary>
		///     Removes all log lines.
		/// </summary>
		public void Clear()
		{
			lock (_syncRoot)
			{
				if (_lines.Count > 0)
				{
					_lines.Clear();
					MaxCharactersPerLine = 0;
					StartTimestamp = null;
					Touch();

					_listeners.Reset();
				}
			}
		}

		/// <summary>
		///     Removes everything from the given index onwards until the end.
		/// </summary>
		/// <param name="index"></param>
		public void RemoveFrom(LogLineIndex index)
		{
			lock (_syncRoot)
			{
				if (index < 0)
				{
					Log.WarnFormat("Invalid index '{0}'", index);
					return;
				}

				if (index > _lines.Count)
				{
					Log.WarnFormat("Invalid index '{0}', Count is '{1}'", index, _lines.Count);
					return;
				}

				var available = _lines.Count - index;
				_lines.RemoveRange((int) index, available);
				_listeners.Invalidate((int) index, available);
				Touch();
			}
		}

		private void Touch()
		{
			LastModified = DateTime.Now;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rawContent"></param>
		public void AddEntry(string rawContent)
		{
			AddEntry(rawContent, LevelFlags.None);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rawContent"></param>
		/// <param name="level"></param>
		public void AddEntry(string rawContent, LevelFlags level)
		{
			AddEntry(rawContent, level, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rawContent"></param>
		/// <param name="level"></param>
		/// <param name="timestamp"></param>
		public void AddEntry(string rawContent, LevelFlags level, DateTime? timestamp)
		{
			lock (_syncRoot)
			{
				int logEntryIndex;
				if (_lines.Count > 0)
				{
					var last = _lines[_lines.Count - 1];
					logEntryIndex = last.LogEntryIndex + 1;
				}
				else
				{
					logEntryIndex = 0;
					StartTimestamp = timestamp;
				}

				_lines.Add(new LogLine(_lines.Count, logEntryIndex, rawContent, level, timestamp));
				MaxCharactersPerLine = Math.Max(MaxCharactersPerLine, rawContent.Length);
				Touch();

				_listeners.OnRead(_lines.Count);
			}
		}

		/// <summary>
		///     Adds a multi line log entry to this log file.
		/// </summary>
		/// <param name="level"></param>
		/// <param name="timestamp"></param>
		/// <param name="lines"></param>
		public void AddMultilineEntry(LevelFlags level, DateTime? timestamp, params string[] lines)
		{
			lock (_syncRoot)
			{
				int logEntryIndex;
				if (_lines.Count > 0)
				{
					var last = _lines[_lines.Count - 1];
					logEntryIndex = last.LogEntryIndex + 1;
				}
				else
				{
					logEntryIndex = 0;
					StartTimestamp = timestamp;
				}

				foreach (var line in lines)
				{
					_lines.Add(new LogLine(_lines.Count, logEntryIndex, line, level, timestamp));
					MaxCharactersPerLine = Math.Max(MaxCharactersPerLine, line.Length);
				}
				Touch();

				_listeners.OnRead(_lines.Count);
			}
		}

		/// <summary>
		///     Adds <paramref name="count" /> amount of empty lines to this log file.
		/// </summary>
		/// <param name="count"></param>
		public void AddEmptyEntries(int count)
		{
			for (int i = 0; i < count; ++i)
			{
				AddEntry(string.Empty, LevelFlags.None);
			}
		}
	}
}