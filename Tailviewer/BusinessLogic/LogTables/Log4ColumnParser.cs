using System;
using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.LogTables
{
	/// <summary>
	///     Takes care of parsing a log4net pattern that contains exactly *one* pattern such as '%logger'.
	///     This class is mostly concerned with consuming fixed characters before and after the actual pattern data.
	///     The actual parsing of the data (such as timestamp, etc...) is delegated to a dedicated parser for that type.
	/// </summary>
	public sealed class Log4ColumnParser
		: IColumnParser
	{
		private static readonly Dictionary<string, ColumnType> ColumnTypes;

		private readonly string _name;
		private readonly string _pattern;
		private readonly ColumnType _type;

		static Log4ColumnParser()
		{
			ColumnTypes = new Dictionary<string, ColumnType>
				{
					{"timestamp", ColumnType.Timestamp},
					{"r", ColumnType.Timestamp},
					{"date", ColumnType.Date},
					{"d", ColumnType.Date},
					{"utcdate", ColumnType.UtcDate},
					{"thread", ColumnType.Thread},
					{"t", ColumnType.Thread},
					{"identity", ColumnType.Identity},
					{"u", ColumnType.Identity},
					{"level", ColumnType.Level},
					{"p", ColumnType.Level},
					{"logger", ColumnType.Logger},
					{"c", ColumnType.Logger},
					{"message", ColumnType.Message},
					{"m", ColumnType.Message},
					{"location", ColumnType.Location},
					{"l", ColumnType.Location},
					{"file", ColumnType.File},
					{"F", ColumnType.File},
					{"type", ColumnType.Type},
					{"C", ColumnType.Type},
					{"class", ColumnType.Type},
					{"line", ColumnType.Line},
					{"L", ColumnType.Line},
					{"method", ColumnType.Method},
					{"M", ColumnType.Method},
				};
		}

		private Log4ColumnParser(string pattern, string name, ColumnType type)
		{
			_pattern = pattern;
			_name = name;
			_type = type;
		}

		public string Pattern
		{
			get { return _pattern; }
		}

		public string Name
		{
			get { return _name; }
		}

		public ColumnType Type
		{
			get { return _type; }
		}

		public object Parse(string line, int startIndex, out int numCharactersConsumed)
		{
			throw new NotImplementedException();
		}

		public static Log4ColumnParser[] Create(string pattern)
		{
			var parsers = new List<Log4ColumnParser>();
			Log4ColumnParser parser;
			int startIndex = 0;
			int columnLength;
			while ((parser = Create(pattern, startIndex, out columnLength)) != null && columnLength > 0)
			{
				parsers.Add(parser);
				startIndex += columnLength;
			}

			return parsers.ToArray();
		}

		public static Log4ColumnParser Create(string pattern, int startIndex, out int patternLength)
		{
			if (pattern == null)
				throw new ArgumentNullException("pattern");

			int columnStartIndex = pattern.IndexOf('%', startIndex);
			if (columnStartIndex != -1)
			{
				int columnEndIndex = pattern.IndexOf('%', columnStartIndex + 1);
				if (columnEndIndex == -1)
					columnEndIndex = pattern.Length;

				string subPattern = pattern.Substring(startIndex, columnEndIndex - startIndex);
				int nameStartIndex = pattern.IndexOf(char.IsLetter, columnStartIndex + 1, columnEndIndex - columnStartIndex - 1);
				if (nameStartIndex != -1)
				{
					int nameEndIndex = pattern.IndexOf(c => !char.IsLetter(c), nameStartIndex);
					if (nameEndIndex == -1)
						nameEndIndex = columnEndIndex;
					string name = pattern.Substring(nameStartIndex, nameEndIndex - nameStartIndex);
					ColumnType type;
					ColumnTypes.TryGetValue(name, out type);

					patternLength = columnEndIndex - startIndex;
					return new Log4ColumnParser(subPattern, name, type);
				}

				patternLength = pattern.Length - startIndex;
				return new Log4ColumnParser(subPattern, "<Unknown>", ColumnType.Unknown);
			}

			patternLength = 0;
			return null;
		}
	}
}