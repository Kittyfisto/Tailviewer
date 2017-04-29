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

		public Size FileSize => Size.Zero;

		public bool Exists => true;

		public bool EndOfSourceReached => true;

		public int Count => _lines.Count;

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
		}
	}
}