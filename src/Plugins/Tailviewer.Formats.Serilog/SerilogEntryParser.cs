using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tailviewer.Formats.Serilog.Matchers;
using Tailviewer.Plugins;

namespace Tailviewer.Formats.Serilog
{
	/// <summary>
	///     Responsible for parsing a singular log line into a <see cref="ILogEntry" /> by using
	///     a Serilog format specification such as "{Timestamp:dd/MM/yyyy HH:mm:ss K} [{Level}] {Message}".
	/// </summary>
	public sealed class SerilogEntryParser
		: ILogEntryParser
	{
		private readonly IReadOnlyList<ISerilogMatcher> _matchers;
		private readonly Regex _regex;
		private readonly IReadOnlyList<IColumnDescriptor> _columns;

		public SerilogEntryParser(string serilogFormat)
		{
			_regex = new Regex(ToRegex(serilogFormat ?? string.Empty, out _matchers));
			_columns = _matchers.Select(x => x.Column).ToList();
		}

		public static string ToRegex(string serilogFormat, out IReadOnlyList<ISerilogMatcher> matchers)
		{
			var buffer = new StringBuilder("^");
			var matchersInOrder = new List<ISerilogMatcher>();
			var groupIndex = 1;
			for (var i = 0; i < serilogFormat.Length;)
			{
				var first = serilogFormat.IndexOf(value: '{', i);
				if (first == -1)
				{
					var remaining = serilogFormat.Substring(i);
					buffer.Append(Regex.Escape(remaining));
					break;
				}

				var next = serilogFormat.IndexOf(value: '}', first + 1);
				if (next == -1)
					throw new ArgumentException($"Format '{serilogFormat}' is missing a closing bracket");

				var specifier = serilogFormat.Substring(first + 1, next - first - 1);
				var matcher = CreateMatcher(specifier, groupIndex);
				matchersInOrder.Add(matcher);

				var prefix = serilogFormat.Substring(i, first - i);
				buffer.Append(Regex.Escape(prefix));
				buffer.Append(matcher.Regex);

				i = next + 1;
				groupIndex += matcher.NumGroups;
			}

			matchers = matchersInOrder;
			return buffer.ToString();
		}

		private static ISerilogMatcher CreateMatcher(string format, int groupIndex)
		{
			var separator = format.IndexOf(value: ':');
			if (separator != -1)
				return CreateMatcher(format.Substring(startIndex: 0, separator),
				                     format.Substring(separator + 1),
				                     groupIndex);

			return CreateMatcher(format, "", groupIndex);
		}

		private static ISerilogMatcher CreateMatcher(string type, string specifier, int groupIndex)
		{
			switch (type)
			{
				case "Timestamp":
					return new TimestampMatcher(specifier, groupIndex);
				case "Level":
					return new LevelMatcher(specifier, groupIndex);
				case "Message":
					return new MessageMatcher(specifier, groupIndex);
				default:
					throw new ArgumentException($"Unknown type: {type}");
			}
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
		}

		public IReadOnlyLogEntry Parse(IReadOnlyLogEntry logEntry)
		{
			if (!TryParse(logEntry.RawContent, out var parsedLogEntry))
				return logEntry;

			return parsedLogEntry;
		}

		public IEnumerable<IColumnDescriptor> Columns
		{
			get { return _columns; }
		}

		#endregion

		public bool TryParse(string rawContent, out IReadOnlyLogEntry logEntry)
		{
			if (rawContent == null)
			{
				logEntry = null;
				return false;
			}

			var match = _regex.Match(rawContent);
			if (!match.Success)
			{
				logEntry = null;
				return false;
			}

			var parsedLogEntry = new SerilogEntry(rawContent);
			foreach (var matcher in _matchers) matcher.MatchInto(match, parsedLogEntry);

			logEntry = parsedLogEntry;
			return true;
		}
	}
}