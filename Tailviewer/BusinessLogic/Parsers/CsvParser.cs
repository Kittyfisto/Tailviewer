using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.BusinessLogic.Parsers
{
	/// <summary>
	///     Is capable of splitting a log file according to a given delimiter into a table.
	/// </summary>
	public sealed class CsvParser
		: ILogFileParser
	{
		private readonly char _delimiter;

		public CsvParser(char delimiter)
		{
			_delimiter = delimiter;
		}

		[Pure]
		public LogTableRow Parse(LogLine line)
		{
			var fields = line.Message.Split(new[] {_delimiter});
			return new LogTableRow((IEnumerable<object>)fields);
		}

		[Pure]
		public LogTableRow Parse(IEnumerable<LogLine> entry)
		{
			throw new NotImplementedException();
		}
	}
}