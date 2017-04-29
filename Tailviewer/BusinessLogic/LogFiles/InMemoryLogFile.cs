using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Metrolib;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	/// TODO: Use this implementation is tests, where applicable (should reduce number of mocks...).
	/// </summary>
	public sealed class InMemoryLogFile
		: ILogFile
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly object _syncRoot;
		private readonly List<LogLine> _lines;
		private readonly LogFileListenerCollection _listeners;
		private int _maxCharactersPerLine;
		private DateTime _lastModified;
		private DateTime? _startTimestamp;

		public InMemoryLogFile()
		{
			_syncRoot = new object();
			_lines = new List<LogLine>();
			_listeners = new LogFileListenerCollection(this);
		}

		public void Dispose()
		{
			
		}

		public DateTime? StartTimestamp => _startTimestamp;

		public DateTime LastModified => _lastModified;

		public Size FileSize => Size.Zero;

		public bool Exists => true;

		public bool EndOfSourceReached => true;

		public int Count => _lines.Count;

		public int MaxCharactersPerLine => _maxCharactersPerLine;

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
					_maxCharactersPerLine = 0;
					_startTimestamp = null;
					Touch();
				}
			}
		}

		public void RemoveRange(LogLineIndex index, int count)
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
				int actualCount = Math.Min(available, count);
				if (actualCount != count)
				{
					Log.WarnFormat("Trying to remove ({0}) more lines than available ({1})", count, available);
				}

				_lines.RemoveRange((int) index, actualCount);
				_listeners.Invalidate((int) index, actualCount);
				Touch();
			}
		}

		private void Touch()
		{
			_lastModified = DateTime.Now;
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
					_startTimestamp = timestamp;
				}

				_lines.Add(new LogLine(_lines.Count, index, message, level, timestamp));
				_maxCharactersPerLine = Math.Max(_maxCharactersPerLine, message.Length);
				Touch();
			}
		}
	}
}