using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Serilog.Matchers;

namespace Tailviewer.Serilog
{
	public sealed class SerilogFileParser
		: ITextLogFileParser
	{
		private readonly Regex _regex;
		private readonly IReadOnlyList<ISerilogMatcher> _matchers;

		public static string ToRegex(string serilogFormat, out IReadOnlyList<ISerilogMatcher> matchers)
		{
			var buffer = new StringBuilder("^");
			var matchersInOrder = new List<ISerilogMatcher>();
			int groupIndex = 1;
			for (int i = 0; i < serilogFormat.Length;)
			{
				int first = serilogFormat.IndexOf('{', i);
				if (first == -1)
				{
					var remaining = serilogFormat.Substring(i);
					buffer.Append(Regex.Escape(remaining));
					break;
				}

				int next = serilogFormat.IndexOf('}', first + 1);
				if (next == -1)
					throw new ArgumentException($"Format '{serilogFormat}' is missing a closing bracket");

				var specifier = serilogFormat.Substring(first + 1, next - first - 1);
				var matcher = CreateMatcher(specifier, groupIndex);
				matchersInOrder.Add(matcher);

				var prefix = serilogFormat.Substring(i, first - i);
				buffer.Append(Regex.Escape(prefix));
				buffer.Append(matcher.Regex);

				i = next + 1;
				groupIndex += matcher.NumCaptures;
			}

			matchers = matchersInOrder;
			return buffer.ToString();
		}

		public SerilogFileParser(string serilogFormat)
		{
			_regex = new Regex(ToRegex(serilogFormat, out _matchers));
		}

		#region Implementation of IDisposable

		public void Dispose()
		{}

		public IReadOnlyLogEntry Parse(IReadOnlyLogEntry logEntry)
		{
			var content = logEntry.RawContent;
			var match = _regex.Match(content);
			if (!match.Success)
				return logEntry;

			var parsedLogEntry = new SerilogEntry();
			foreach (var matcher in _matchers)
			{
				matcher.MatchInto(match, parsedLogEntry);
			}

			return parsedLogEntry;
		}

		#endregion

		private static ISerilogMatcher CreateMatcher(string format, int groupIndex)
		{
			int separator = format.IndexOf(':');
			if (separator != -1)
			{
				return CreateMatcher(format.Substring(0, separator),
				               format.Substring(separator + 1),
				               groupIndex);
			}

			return CreateMatcher(format, "", groupIndex);
		}

		private static ISerilogMatcher CreateMatcher(string type, string specifier, int groupIndex)
		{
			switch (type)
			{
				case "Timestamp":
					return new TimestampMatcher(specifier, groupIndex);
				case "Level":
					return new SerilogLevelMatcher(specifier, groupIndex);
				default:
					throw new ArgumentException($"Unknown type: {type}");
			}
		}
	}
}