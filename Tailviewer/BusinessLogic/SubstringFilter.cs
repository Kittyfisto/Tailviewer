using System;
using System.Diagnostics.Contracts;

namespace Tailviewer.BusinessLogic
{
	/// <summary>
	/// A filter that looks for a substring in a (possibly) bigger string.
	/// </summary>
	internal sealed class SubstringFilter
		: IFilter
	{
		public readonly string StringFilter;
		public readonly StringComparison Comparison;

		public SubstringFilter(string stringFilter, bool ignoreCase)
		{
			StringFilter = stringFilter;
			Comparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
		}

		[Pure]
		public bool PassesFilter(LogEntry logEntry)
		{
			int idx = logEntry.Message.IndexOf(StringFilter, Comparison);
			if (idx == -1)
				return false;

			return true;
		}
	}
}