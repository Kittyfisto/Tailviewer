using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogTables.Parsers
{
	/// <summary>
	///     Is capable of parsing a log file according to a given log4(net/j/cpp) pattern into a table.
	/// </summary>
	public sealed class Log4PatternParser
		: ILogFileParser
	{
		private readonly string _pattern;

		/// <summary>
		///     Initializes this parser with the given pattern.
		/// </summary>
		/// <param name="pattern"></param>
		public Log4PatternParser(string pattern)
		{
			_pattern = pattern;

			var parsers = new List<Log4ColumnParser>();
			Log4ColumnParser parser;
			var startIndex = 0;
			int columnLength;
			while ((parser = Log4ColumnParser.Create(pattern, startIndex, out columnLength)) != null && columnLength > 0)
			{
				parsers.Add(parser);
				startIndex += columnLength;
			}

			Parsers = parsers.ToArray();
		}

		/// <summary>
		///     The pattern used to parses a log line.
		/// </summary>
		public string Pattern => _pattern;

		/// <summary>
		///     The individual parsers for each column, ordered from first to last column.
		/// </summary>
		public Log4ColumnParser[] Parsers { get; }

		/// <inheritdoc />
		[Pure]
		public LogEntry Parse(LogLine line)
		{
			var fields = new object[Parsers.Length];

			var message = line.Message;
			var startIndex = 0;
			for (var i = 0; i < Parsers.Length; ++i)
			{
				int length;
				fields[i] = Parsers[i].Parse(message, startIndex, out length);
				startIndex += length;
			}

			return new LogEntry(fields);
		}

		/// <inheritdoc />
		[Pure]
		public LogEntry Parse(IEnumerable<LogLine> entry)
		{
			throw new NotImplementedException();
		}
	}
}