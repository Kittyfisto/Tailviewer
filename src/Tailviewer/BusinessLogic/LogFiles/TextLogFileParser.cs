using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.BusinessLogic.LogFiles
{
	internal sealed class TextLogFileParser
		: ITextLogFileParser
	{
		private readonly ITimestampParser _timestampParser;
		private int _numTimestampSuccess;
		private int _numSuccessiveTimestampFailures;

		public TextLogFileParser(ITimestampParser timestampParser)
		{
			_timestampParser = timestampParser ?? throw new ArgumentNullException(nameof(timestampParser));
		}

		#region Implementation of IDisposable

		public void Dispose()
		{}

		#endregion

		#region Implementation of ITextLogFileParser

		public IReadOnlyLogEntry Parse(IReadOnlyLogEntry logEntry)
		{
			var line = logEntry.RawContent;
			var level = LogLine.DetermineLevelFromLine(line);
			var timestamp = ParseTimestamp(line);
			return new ParsedLogEntry(logEntry, level, timestamp);
		}

		#endregion

		sealed class ParsedLogEntry
			: IReadOnlyLogEntry
		{
			private readonly IReadOnlyLogEntry _inner;
			private readonly LevelFlags _logLevel;
			private readonly DateTime? _timestamp;

			public ParsedLogEntry(IReadOnlyLogEntry inner, LevelFlags logLevel, DateTime? timestamp)
			{
				_inner = inner;
				_logLevel = logLevel;
				_timestamp = timestamp;
			}

			#region Implementation of IReadOnlyLogEntry

			public string RawContent
			{
				get { return _inner.RawContent; }
			}

			public LogLineIndex Index
			{
				get { return _inner.Index; }
			}

			public LogLineIndex OriginalIndex
			{
				get { return _inner.OriginalIndex; }
			}

			public LogEntryIndex LogEntryIndex
			{
				get { return _inner.LogEntryIndex; }
			}

			public int LineNumber
			{
				get { return _inner.LineNumber; }
			}

			public int OriginalLineNumber
			{
				get { return _inner.OriginalLineNumber; }
			}

			public LevelFlags LogLevel
			{
				get { return _logLevel; }
			}

			public DateTime? Timestamp
			{
				get { return _timestamp; }
			}

			public TimeSpan? ElapsedTime
			{
				get { return _inner.ElapsedTime; }
			}

			public TimeSpan? DeltaTime
			{
				get { return _inner.DeltaTime; }
			}

			public T GetValue<T>(ILogFileColumn<T> column)
			{
				if (Equals(column, LogFileColumns.Timestamp))
				{
					return (T)(object)_timestamp;
				}

				if (Equals(column, LogFileColumns.LogLevel))
				{
					return (T)(object)_logLevel;
				}

				return _inner.GetValue(column);
			}

			public bool TryGetValue<T>(ILogFileColumn<T> column, out T value)
			{
				if (Equals(column, LogFileColumns.Timestamp))
				{
					value = (T)(object)_timestamp;
					return true;
				}

				if (Equals(column, LogFileColumns.LogLevel))
				{
					value = (T)(object)_logLevel;
					return true;
				}

				return _inner.TryGetValue(column, out value);
			}

			public object GetValue(ILogFileColumn column)
			{
				if (Equals(column, LogFileColumns.Timestamp))
				{
					return _timestamp;
				}

				if (Equals(column, LogFileColumns.LogLevel))
				{
					return _logLevel;
				}

				return _inner.GetValue(column);
			}

			public bool TryGetValue(ILogFileColumn column, out object value)
			{
				if (Equals(column, LogFileColumns.Timestamp))
				{
					value = _timestamp;
					return true;
				}

				if (Equals(column, LogFileColumns.LogLevel))
				{
					value = _logLevel;
					return true;
				}

				return _inner.TryGetValue(column, out value);
			}

			public IReadOnlyList<ILogFileColumn> Columns
			{
				get { return _inner.Columns.Concat(new ILogFileColumn[]{LogFileColumns.LogLevel, LogFileColumns.Timestamp}).ToList(); }
			}

			#endregion
		}

		private DateTime? ParseTimestamp(string line)
		{
			// If we stumble upon a file that doesn't contain a single timestamp in the first hundred log lines,
			// then we will just call it a day and never try again...
			// This obviously opens the possibility for not being able to detect valid timestamps in a file, however
			// this is outweighed by being able to read a file without memory FAST. The current algorithm to detect
			// the position and format is so slow that I can read about 1k lines of random data which is pretty bad...
			if (_numTimestampSuccess == 0 &&
			    _numSuccessiveTimestampFailures >= 100)
			{
				return null;
			}

			DateTime timestamp;
			if (_timestampParser.TryParse(line, out timestamp))
			{
				++_numTimestampSuccess;
				_numSuccessiveTimestampFailures = 0;
				return timestamp;
			}

			++_numSuccessiveTimestampFailures;
			return null;
		}
	}
}