using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Tailviewer.BusinessLogic
{
	/// <summary>
	/// A filter that looks for a substring in a (possibly) bigger string.
	/// </summary>
	internal sealed class SubstringFilter
		: ILogEntryFilter
	{
		public readonly string StringFilter;
		public readonly StringComparison Comparison;

		public SubstringFilter(string stringFilter, bool ignoreCase)
		{
			StringFilter = stringFilter;
			Comparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
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

		[Pure]
		public bool PassesFilter(LogLine logLine)
		{
			int idx = logLine.Message.IndexOf(StringFilter, Comparison);
			if (idx == -1)
				return false;

			return true;
		}
	}
}