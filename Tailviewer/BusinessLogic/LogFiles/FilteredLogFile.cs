using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Metrolib;
using Tailviewer.BusinessLogic.Filters;

namespace Tailviewer.BusinessLogic.LogFiles
{
	public sealed class FilteredLogFile
		: AbstractLogFile
		  , ILogFileListener
	{
		private const int BatchSize = 1000;

		private readonly ILogEntryFilter _filter;
		private readonly List<int> _indices;
		private readonly ConcurrentQueue<LogFileSection> _pendingModifications;
		private readonly ILogFile _source;
		private LogFileSection _fullSourceSection;
		private int _maxCharactersPerLine;

		public FilteredLogFile(ILogFile source, ILogEntryFilter filter)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (filter == null) throw new ArgumentNullException("filter");

			_source = source;
			_filter = filter;
			_pendingModifications = new ConcurrentQueue<LogFileSection>();
			_indices = new List<int>();
		}

		public override bool Exists
		{
			get { return _source.Exists; }
		}

		public override bool EndOfSourceReached
		{
			get { return _source.EndOfSourceReached & base.EndOfSourceReached; }
		}

		public override DateTime? StartTimestamp
		{
			get { return _source.StartTimestamp; }
		}

		public override DateTime LastModified
		{
			get { throw new NotImplementedException(); }
		}

		public override Size FileSize
		{
			get { throw new NotImplementedException(); }
		}

		public override int Count
		{
			get
			{
				lock (_indices)
				{
					return _indices.Count;
				}
			}
		}

		public override int MaxCharactersPerLine
		{
			get { return _maxCharactersPerLine; }
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			_pendingModifications.Enqueue(section);
			ResetEndOfSourceReached();
		}

		public override void GetSection(LogFileSection section, LogLine[] dest)
		{
			if (section.Index < 0)
				throw new ArgumentOutOfRangeException("section.Index");
			if (section.Count < 0)
				throw new ArgumentOutOfRangeException("section.Count");
			if (dest == null)
				throw new ArgumentNullException("dest");
			if (dest.Length < section.Count)
				throw new ArgumentOutOfRangeException("section.Count");

			lock (_indices)
			{
				if (section.Index + section.Count > _indices.Count)
					throw new ArgumentOutOfRangeException("section");

				for (int i = 0; i < section.Count; ++i)
				{
					LogLineIndex index = section.Index + i;
					int sourceIndex = _indices[(int) index];
					LogLine line = _source.GetLine(sourceIndex);
					dest[i] = new LogLine((int)index, line.LogEntryIndex, line.Message, line.Level, line.Timestamp);
				}
			}
		}

		public override LogLine GetLine(int index)
		{
			lock (_indices)
			{
				int sourceIndex = _indices[index];
				var line = _source.GetLine(sourceIndex);
				return new LogLine(index, line.LogEntryIndex, line.Message, line.Level, line.Timestamp);
			}
		}

		public void Start(TimeSpan maximumWaitTime)
		{
			_source.AddListener(this, maximumWaitTime, BatchSize);
			StartTask();
		}

		public override string ToString()
		{
			return string.Format("{0} (Filtered)", _source);
		}

		protected override void Run(CancellationToken token)
		{
			var entries = new LogLine[BatchSize];
			int currentSourceIndex = 0;
			var lastLogEntry = new List<LogLine>();

			while (!token.IsCancellationRequested)
			{
				LogFileSection section;
				while (_pendingModifications.TryDequeue(out section))
				{
					if (section.IsReset)
					{
						Clear();
						lastLogEntry.Clear();
						currentSourceIndex = 0;
					}
					else if (section.InvalidateSection)
					{
						LogLineIndex startIndex = section.Index;
						_fullSourceSection = new LogFileSection(0, (int) startIndex);

						if (currentSourceIndex > _fullSourceSection.LastIndex)
							currentSourceIndex = (int) section.Index;

						Invalidate(currentSourceIndex);
						RemoveInvalidatedLines(lastLogEntry, currentSourceIndex);
					}
					else
					{
						_fullSourceSection = LogFileSection.MinimumBoundingLine(_fullSourceSection, section);
					}
				}

				if (_fullSourceSection.IsEndOfSection(currentSourceIndex))
				{
					TryAddLogLine(lastLogEntry);
					Listeners.OnRead(_indices.Count);

					SetEndOfSourceReached();

					// There's no more data, let's wait for more (or until we're disposed)
					token.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(10));
				}
				else
				{
					int remaining = _fullSourceSection.Index + _fullSourceSection.Count - currentSourceIndex;
					int nextCount = Math.Min(remaining, BatchSize);
					var nextSection = new LogFileSection(currentSourceIndex, nextCount);
					_source.GetSection(nextSection, entries);

					for (int i = 0; i < nextCount; ++i)
					{
						if (token.IsCancellationRequested)
							break;

						LogLine line = entries[i];
						if (lastLogEntry.Count == 0 || lastLogEntry[0].LogEntryIndex == line.LogEntryIndex)
						{
							lastLogEntry.Add(line);
						}
						else if (line.LogEntryIndex != lastLogEntry[0].LogEntryIndex)
						{
							TryAddLogLine(lastLogEntry);
							lastLogEntry.Clear();
							lastLogEntry.Add(line);
						}
					}

					currentSourceIndex += nextCount;
				}
			}
		}

		private static void RemoveInvalidatedLines(List<LogLine> lastLogEntry, int currentSourceIndex)
		{
			while (lastLogEntry.Count > 0)
			{
				int i = lastLogEntry.Count - 1;
				LogLine line = lastLogEntry[i];
				if (line.LineIndex >= currentSourceIndex)
				{
					lastLogEntry.RemoveAt(i);
				}
				else
				{
					break;
				}
			}
		}

		private void Invalidate(int currentSourceIndex)
		{
			int numRemoved = 0;
			lock (_indices)
			{
				while (_indices.Count > 0)
				{
					int i = _indices.Count - 1;
					int sourceIndex = _indices[i];
					if (sourceIndex >= currentSourceIndex)
					{
						_indices.RemoveAt(i);
						++numRemoved;
					}
					else
					{
						break;
					}
				}
			}
			Listeners.Invalidate(_indices.Count, numRemoved);
		}

		private void Clear()
		{
			_fullSourceSection = new LogFileSection();
			lock (_indices)
			{
				_indices.Clear();
			}
			Listeners.OnRead(-1);
		}

		private bool TryAddLogLine(List<LogLine> logEntry)
		{
			if (_indices.Count > 0 && logEntry.Count > 0 &&
			    _indices[_indices.Count - 1] == logEntry[logEntry.Count - 1].LineIndex)
				return true;

			if (_filter.PassesFilter(logEntry))
			{
				lock (_indices)
				{
					foreach (LogLine line in logEntry)
					{
						_indices.Add(line.LineIndex);
						_maxCharactersPerLine = Math.Max(_maxCharactersPerLine, line.Message.Length);
					}
				}
				Listeners.OnRead(_indices.Count);
				return true;
			}

			if (logEntry.Count > 0)
			{
				int n = 0;
			}

			return false;
		}
	}
}