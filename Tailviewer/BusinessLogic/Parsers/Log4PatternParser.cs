using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.BusinessLogic.Parsers
{
	/// <summary>
	///     Is capable of parsing a log file according to a given log4(net/j/cpp) pattern into a table.
	/// </summary>
	public sealed class Log4PatternParser
		: ILogFileParser
	{
		private readonly Log4ColumnParser[] _columns;
		private readonly string _pattern;

		public Log4PatternParser(string pattern)
		{
			_pattern = pattern;

			var parsers = new List<Log4ColumnParser>();
			Log4ColumnParser parser;
			int startIndex = 0;
			int columnLength;
			while ((parser = Log4ColumnParser.Create(pattern, startIndex, out columnLength)) != null && columnLength > 0)
			{
				parsers.Add(parser);
				startIndex += columnLength;
			}

			_columns = parsers.ToArray();
		}

		public string Pattern
		{
			get { return _pattern; }
		}

		public Log4ColumnParser[] Columns
		{
			get { return _columns; }
		}

		[Pure]
		public LogTableRow Parse(LogLine line)
		{
			var fields = new object[_columns.Length];

			string message = line.Message;
			int startIndex = 0;
			for (int i = 0; i < _columns.Length; ++i)
			{
				int length;
				fields[i] = _columns[i].Parse(message, startIndex, out length);
				startIndex += length;
			}

			return new LogTableRow(fields);
		}

		[Pure]
		public LogTableRow Parse(IEnumerable<LogLine> entry)
		{
			throw new NotImplementedException();
		}
	}
}