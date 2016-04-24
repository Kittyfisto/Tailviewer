using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Filters
{
	/// <summary>
	///     A filter that looks for a substring in a (possibly) bigger string.
	/// </summary>
	internal sealed class SubstringFilter
		: ILogEntryFilter
	{
		public readonly StringComparison Comparison;
		public readonly string StringFilter;

		public SubstringFilter(string stringFilter, bool ignoreCase)
		{
			StringFilter = stringFilter;
			Comparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
		}

		public bool PassesFilter(IEnumerable<LogLine> logEntry)
		{
			// ReSharper disable LoopCanBeConvertedToQuery
			foreach (LogLine logLine in logEntry)
				// ReSharper restore LoopCanBeConvertedToQuery
			{
				if (PassesFilter(logLine))
					return true;
			}

			return false;
		}

		[Pure]
		public bool PassesFilter(LogLine logLine)
		{
			int idx = logLine.Message.IndexOf(StringFilter, Comparison);
			if (idx == -1)
				return false;

			return true;
		}

		public List<FilterMatch> Match(LogLine line)
		{
			var ret = new List<FilterMatch>();
			Match(line, ret);
			return ret;
		}

		public void Match(LogLine line, List<FilterMatch> matches)
		{
			var message = line.Message;
			int startIndex = 0;
			do
			{
				startIndex = message.IndexOf(StringFilter, startIndex, Comparison);
				if (startIndex < 0)
					break;

				var length = StringFilter.Length;
				matches.Add(new FilterMatch(startIndex, length));
				startIndex += length;
			} while (startIndex < message.Length - 1);
		}
	}
}