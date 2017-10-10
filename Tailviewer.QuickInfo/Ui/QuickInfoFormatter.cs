using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using Tailviewer.Core.Settings;
using Tailviewer.QuickInfo.BusinessLogic;

namespace Tailviewer.QuickInfo.Ui
{
	/// <summary>
	///     Responsible for formatting a quick info result as per
	///     <see cref="QuickInfoConfiguration" /> and <see cref="QuickInfoViewConfiguration" />.
	/// </summary>
	public sealed class QuickInfoFormatter
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);

		private readonly string _filterExpression;
		private readonly Regex _regex;

		public QuickInfoFormatter(QuickInfoConfiguration configuration)
		{
			_filterExpression = configuration.FilterValue;
			switch (configuration.MatchType)
			{
				case FilterMatchType.SubstringFilter:
				case FilterMatchType.WildcardFilter:
					break;

				case FilterMatchType.RegexpFilter:
					_regex = TryCreateRegex(_filterExpression);
					break;
			}
		}

		private static Regex TryCreateRegex(string filterValue)
		{
			if (filterValue == null)
				return null;

			try
			{
				return new Regex(filterValue, RegexOptions.Compiled, RegexTimeout);
			}
			catch (Exception e)
			{
				Log.DebugFormat("Unable to create regular expression from '{0}': {1}", filterValue, e);
				return null;
			}
		}

		[Pure]
		public string Format(Tailviewer.QuickInfo.BusinessLogic.QuickInfo result, string format)
		{
			var builder = new StringBuilder(format);
			builder.Replace("{message}", result.Message ?? "N/A");
			builder.Replace("{timestamp}", result.Timestamp != null ? result.Timestamp.ToString() : "N/A");

			if (_regex != null)
			{
				if (result.Message != null)
				{
					try
					{
						var match = _regex.Match(result.Message);
						ReplaceMatches(builder, match);
					}
					catch (RegexMatchTimeoutException e)
					{
						Log.WarnFormat("Regular expression '{0}' timed out: {1}", _filterExpression, e.Message);
					}
					catch (Exception e)
					{
						Log.ErrorFormat("Caught unexpected exception: {0}", e);
					}
				}
			}

			var formattedValue = builder.ToString();
			return formattedValue;
		}

		private static void ReplaceMatches(StringBuilder builder, Match match)
		{
			for (var i = 0; i < match.Groups.Count; ++i)
			{
				var s = "{" + i + "}";
				var group = match.Groups[i];
				builder.Replace(s, group.Value);
			}
		}
	}
}