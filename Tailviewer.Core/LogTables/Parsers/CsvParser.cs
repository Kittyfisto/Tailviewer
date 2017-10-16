using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogTables.Parsers
{
	/// <summary>
	///     Is capable of splitting a log file according to a given delimiter into a table.
	/// </summary>
	public sealed class CsvParser
		: ILogFileParser
	{
		private readonly char _delimiter;

		/// <summary>
		///     Initializes this parser.
		/// </summary>
		/// <param name="delimiter"></param>
		public CsvParser(char delimiter)
		{
			_delimiter = delimiter;
		}

		/// <inheritdoc />
		[Pure]
		public LogEntry Parse(LogLine line)
		{
			var fields = line.Message.Split(_delimiter);
			return new LogEntry((IEnumerable<object>) fields);
		}

		/// <inheritdoc />
		[Pure]
		public LogEntry Parse(IEnumerable<LogLine> entry)
		{
			throw new NotImplementedException();
		}
	}
}