using System.Collections.Generic;
using System.Text.RegularExpressions;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Filters
{
	internal class WildcardFilter : ILogEntryFilter
	{
		private readonly Regex _regex;

		public WildcardFilter(string pattern, bool ignoreCase)
		{
			var options = RegexOptions.Compiled;
			if (ignoreCase)
				options |= RegexOptions.IgnoreCase;

			var regexPattern = Regex.Escape(pattern)
			                        .Replace(@"\*", ".*")
			                        .Replace(@"\?", ".");
			_regex = new Regex(regexPattern, options);
		}

		public bool PassesFilter(IEnumerable<LogLine> logEntry)
		{
			// ReSharper disable LoopCanBeConvertedToQuery
			foreach (var logLine in logEntry)
			// ReSharper restore LoopCanBeConvertedToQuery
			{
				if (PassesFilter(logLine))
					return true;
			}

			return false;
		}

		public bool PassesFilter(LogLine logLine)
		{
			if (_regex.IsMatch(logLine.Message))
				return true;

			return false;
		}
	}
}