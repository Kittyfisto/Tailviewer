using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Parsers;
using Tailviewer.Plugins;

namespace Tailviewer.BusinessLogic.Sources
{
	internal sealed class GenericTextLogEntryParser
		: ILogEntryParser
	{
		private static readonly string[] RemovableCharacters;
		private readonly ITimestampParser _timestampParser;
		private int _numTimestampSuccess;
		private int _numSuccessiveTimestampFailures;
		private readonly IReadOnlyList<IColumnDescriptor> _columns;

		static GenericTextLogEntryParser()
		{
			// We will remove every character from ASCII [0-31] besides the tab character from the line because we can't display them anways.
			// Whenever you modify this collection, pay attention to to also modify the detection loop below. Checking for insertion was a huge
			// hot-spot and I had to unroll one of the loops to make it less so.
			RemovableCharacters = Enumerable.Range(0, 32).Select(x => new string((char) x, 1)).Where(y => y != "\t")
			                                .Concat(new []{"\u007f"}).ToArray();
		}

		public GenericTextLogEntryParser()
			: this(new TimestampParser())
		{ }

		public GenericTextLogEntryParser(ITimestampParser timestampParser)
		{
			_timestampParser = timestampParser ?? throw new ArgumentNullException(nameof(timestampParser));
			_columns = new IColumnDescriptor[] {LogColumns.Timestamp, LogColumns.LogLevel};
		}

		#region Implementation of IDisposable

		public void Dispose()
		{}

		#endregion

		#region Implementation of ITextLogFileParser

		public IReadOnlyLogEntry Parse(IReadOnlyLogEntry logEntry)
		{
			var rawContent = logEntry.RawContent;
			if (string.IsNullOrEmpty(rawContent))
				return logEntry;

			var line = RemoveGarbage(rawContent);
			var level = LogLine.DetermineLevelFromLine(line);
			var timestamp = ParseTimestamp(line);
			return new ParsedLogEntry(logEntry, line, level, timestamp);
		}

		public IEnumerable<IColumnDescriptor> Columns
		{
			get
			{
				return _columns;
			}
		}

		#endregion

		sealed class ParsedLogEntry
			: IReadOnlyLogEntry
		{
			private readonly IReadOnlyLogEntry _inner;
			private readonly string _rawContent;
			private readonly LevelFlags _logLevel;
			private readonly DateTime? _timestamp;

			public ParsedLogEntry(IReadOnlyLogEntry inner, string rawContent, LevelFlags logLevel, DateTime? timestamp)
			{
				_inner = inner;
				_rawContent = rawContent;
				_logLevel = logLevel;
				_timestamp = timestamp;
			}

			#region Implementation of IReadOnlyLogEntry

			public string RawContent
			{
				get { return _rawContent; }
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

			public string OriginalDataSourceName
			{
				get { return _inner.OriginalDataSourceName; }
			}

			public LogLineSourceId SourceId
			{
				get { return _inner.SourceId; }
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

			public string Message
			{
				get
				{
					return _inner.Message;
				}
			}

			public T GetValue<T>(IColumnDescriptor<T> column)
			{
				if (Equals(column, LogColumns.Timestamp))
				{
					return (T)(object)_timestamp;
				}

				if (Equals(column, LogColumns.LogLevel))
				{
					return (T)(object)_logLevel;
				}

				return _inner.GetValue(column);
			}

			public bool TryGetValue<T>(IColumnDescriptor<T> column, out T value)
			{
				if (Equals(column, LogColumns.Timestamp))
				{
					value = (T)(object)_timestamp;
					return true;
				}

				if (Equals(column, LogColumns.LogLevel))
				{
					value = (T)(object)_logLevel;
					return true;
				}

				return _inner.TryGetValue(column, out value);
			}

			public object GetValue(IColumnDescriptor column)
			{
				if (Equals(column, LogColumns.Timestamp))
				{
					return _timestamp;
				}

				if (Equals(column, LogColumns.LogLevel))
				{
					return _logLevel;
				}

				return _inner.GetValue(column);
			}

			public bool TryGetValue(IColumnDescriptor column, out object value)
			{
				if (Equals(column, LogColumns.Timestamp))
				{
					value = _timestamp;
					return true;
				}

				if (Equals(column, LogColumns.LogLevel))
				{
					value = _logLevel;
					return true;
				}

				return _inner.TryGetValue(column, out value);
			}

			public IReadOnlyList<IColumnDescriptor> Columns
			{
				get { return _inner.Columns.Concat(new IColumnDescriptor[]{LogColumns.LogLevel, LogColumns.Timestamp}).ToList(); }
			}

			#endregion
		}

		private string RemoveGarbage(string line)
		{
			bool found = false;

			// Do NOT build this back into a nested loop which checks for presence in the
			// list... It is awfully slow and a huge hot-spot for something which only happens
			// once in a blue moon because some application cannot be bothered to sanitize their
			// goddamn log file.

			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < line.Length; ++i)
			{
				var @char = line[i];
				if ((@char < 32 | @char == '\u007f') & @char != '\t')
				{
					found = true;
					break;
				}
			}

			// We want to AVOID creating a full stringbuilder for every line so we (because it's construction is AWFULLY slow).
			// So instead we opt for a two-pass algorithm where we first inspect if the line contains a removable character
			// (which text log files shouldn't even have in the first place) 
			if (!found)
				return line;

			var builder = new StringBuilder(line);
			// foreach allocates a bunch of memory and we don't want that here...
			// ReSharper disable once ForCanBeConvertedToForeach
			for (var index = 0; index < RemovableCharacters.Length; index++)
			{
				var character = RemovableCharacters[index];
				builder.Replace(character, "");
			}

			return builder.ToString();
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