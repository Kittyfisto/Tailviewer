using System;
using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.LogTables
{
	internal class Log4ColumnParser
		: IColumnParser
	{
		private static readonly Dictionary<string, ColumnType> ColumnTypes;

		private readonly string _name;
		private readonly ColumnType _type;

		static Log4ColumnParser()
		{
			ColumnTypes = new Dictionary<string, ColumnType>
				{
					{"timestamp", ColumnType.Timestamp},
					{"thread", ColumnType.ThreadName},
					{"level", ColumnType.Level},
					{"logger", ColumnType.Logger},
					{"message", ColumnType.Message},
				};
		}

		public Log4ColumnParser(string subPattern)
		{
			if (subPattern == null)
				throw new ArgumentNullException("subPattern");

			if (!subPattern.StartsWith("%"))
				throw new ArgumentException("The given pattern-part must start with '%'");

			int index = subPattern.IndexOf(1, c => !char.IsLetter(c));
			int length = index != -1 ? index : subPattern.Length;
			_name = subPattern.Substring(1, length - 1);
			ColumnTypes.TryGetValue(_name.ToLowerInvariant(), out _type);
		}

		public string Name
		{
			get { return _name; }
		}

		public ColumnType Type
		{
			get { return _type; }
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

			int start = startIndex + 1;
			if (start >= pattern.Length)
			{
				patternLength = 0;
				return null;
			}

			int index = pattern.IndexOf(start, c => c == '%');
			if (index == -1)
			{
				patternLength = pattern.Length - startIndex;
				if (patternLength < 0)
					return null;
			}
			else
			{
				patternLength = index - startIndex;
			}

			string subPattern = pattern.Substring(startIndex, patternLength);
			return new Log4ColumnParser(subPattern);
		}
	}
}