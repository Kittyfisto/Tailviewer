using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Metrolib;

namespace Tailviewer.BusinessLogic.LogFiles
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

		public InMemoryLogFile()
		{
			_syncRoot = new object();
			_lines = new List<LogLine>();
			_listeners = new LogFileListenerCollection(this);
		}

		public void Dispose()
		{
		}

		public DateTime? StartTimestamp { get; private set; }

		public DateTime LastModified { get; private set; }

		public Size FileSize { get; set; }

		public bool Exists => true;

		public bool EndOfSourceReached => true;

		public int Count => _lines.Count;

		public int OriginalCount => Count;

		public int MaxCharactersPerLine { get; private set; }

		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void RemoveListener(ILogFileListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		public void GetSection(LogFileSection section, LogLine[] dest)
		{
			lock (_lines)
			{
				_lines.CopyTo((int) section.Index, dest, 0, section.Count);
			}
		}

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

		public LogLine GetLine(int index)
		{
			lock (_lines)
			{
				return _lines[index];
			}
		}

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

		public void AddEntry(string message, LevelFlags level)
		{
			AddEntry(message, level, null);
		}

		public void AddEntry(string message, LevelFlags level, DateTime? timestamp)
		{
			lock (_syncRoot)
			{
				int index;
				if (_lines.Count > 0)
				{
					var last = _lines[_lines.Count - 1];
					index = last.LogEntryIndex + 1;
				}
				else
				{
					index = 0;
					StartTimestamp = timestamp;
				}

				_lines.Add(new LogLine(_lines.Count, index, message, level, timestamp));
				MaxCharactersPerLine = Math.Max(MaxCharactersPerLine, message.Length);
				Touch();
			}

			_listeners.OnRead(_lines.Count);
		}

		public void AddEntries(int count)
		{
			for (int i = 0; i < count; ++i)
			{
				AddEntry(string.Empty, LevelFlags.None);
			}
		}
	}
}