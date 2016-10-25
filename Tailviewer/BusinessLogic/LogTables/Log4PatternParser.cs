using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.LogTables
{
	/// <summary>
	///     This class is responsible for parsing any given log4(net/j/cpp) pattern.
	///     Using this pattern, it is then able to create a <see cref="LogTableRow" />
	///     from <see cref="LogLine" />.
	/// </summary>
	public sealed class Log4PatternParser
	{
		private static readonly Dictionary<string, ColumnType> ColumnTypes;

		private readonly string _pattern;
		private readonly Log4ColumnParser[] _columns;

		public IEnumerable<IColumnParser> Columns
		{
			get { return _columns; }
		}

		public Log4PatternParser(string pattern)
		{
			_pattern = pattern;
			_columns = Log4ColumnParser.Create(pattern);
		}

		public string Pattern
		{
			get { return _pattern; }
		}

		public override string ToString()
		{
			return _pattern;
		}
	}
}