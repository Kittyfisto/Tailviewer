using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.BusinessLogic.Parsers
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
		private static readonly Regex PatternRegex;

		private readonly int? _maximumLength;
		private readonly int? _minimumLength;
		private readonly string _name;
		private readonly string _pattern;
		private readonly ColumnType _type;
		private readonly ColumnParser _parser;
		private readonly string _format;

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
					{"newline", ColumnType.Newline}
				};

			PatternRegex = new Regex(@"%(-?\d+)?(\.\d+)?([a-zA-Z_][a-zA-Z0-9_]*)({[^}]+})?", RegexOptions.Compiled);
		}

		private Log4ColumnParser(string pattern, string name, string format, ColumnType type, int? minimumLength, int? maximumLength)
		{
			_pattern = pattern;
			_name = name;
			_format = format;
			_type = type;
			_minimumLength = minimumLength;
			_maximumLength = maximumLength;
			_parser = ColumnParser.Create(type, format);
		}

		public int? MinimumLength
		{
			get { return _minimumLength; }
		}

		public int? MaximumLength
		{
			get { return _maximumLength; }
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

			Match match = PatternRegex.Match(pattern, startIndex);
			if (match.Success)
			{
				string name, format;
				int? minimumLength;
				int? maximumLength;
				if (match.Groups.Count == 5)
				{
					name = match.Groups[3].Value;
					format = match.Groups[4].Value;
					minimumLength = null;
					maximumLength = null;
				}
				else if (match.Groups.Count == 6)
				{
					minimumLength = GetLength(match.Groups[1].Value);
					maximumLength = null;
					name = match.Groups[4].Value;
					format = match.Groups[5].Value;
				}
				else if (match.Groups.Count == 7)
				{
					minimumLength = GetLength(match.Groups[1].Value);
					maximumLength = GetLength(match.Groups[2].Value);
					name = match.Groups[5].Value;
					format = match.Groups[6].Value;
				}
				else
				{
					patternLength = 0;
					return null;
				}

				patternLength = match.Index - startIndex + match.Length;
				ColumnType type;
				ColumnTypes.TryGetValue(name, out type);
				string columnPattern = pattern.Substring(startIndex, patternLength);
				return new Log4ColumnParser(columnPattern, name, format, type, minimumLength, maximumLength);
			}

			patternLength = 0;
			return null;
		}

		private static int? GetLength(string value)
		{
			int length;
			if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out length))
				return length;

			return null;
		}
	}
}