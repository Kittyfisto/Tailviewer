using System.Text.RegularExpressions;

namespace Tailviewer.BusinessLogic
{
	internal sealed class RegexFilter
		: IFilter
	{
		private readonly Regex _regex;

		public RegexFilter(string pattern, bool isCaseSensitive)
		{
			var options = RegexOptions.Compiled;
			if (isCaseSensitive)
				options |= RegexOptions.IgnoreCase;

			_regex = new Regex(pattern, options);
		}

		public bool PassesFilter(LogEntry logEntry)
		{
			if (_regex.IsMatch(logEntry.Message))
				return true;

			return false;
		}
	}
}