using System.Text.RegularExpressions;

namespace Tailviewer.BusinessLogic
{
	internal class WildcardFilter : IFilter
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

		public bool PassesFilter(LogEntry logEntry)
		{
			if (_regex.IsMatch(logEntry.Message))
				return true;

			return false;
		}
	}
}