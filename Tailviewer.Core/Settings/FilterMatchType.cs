using System.Text.RegularExpressions;

namespace Tailviewer.Core.Settings
{
	/// <summary>
	///     Defines how the filter value is supposed to be interpreted.
	/// </summary>
	public enum FilterMatchType
	{
		/// <summary>
		///     A simple sub-string search is performed: The filter matches if a line contains the filter value as is
		/// </summary>
		SubstringFilter = 0,

		/// <summary>
		///     The filter is interpreted as being a simple wildcard expression:
		///     "a*b" matches any line containing an a, followed by any character(s) and then b.
		/// </summary>
		WildcardFilter = 1,

		/// <summary>
		///     The filter is interpreted as being a regular expression.
		///     See <see cref="Regex" /> as to what syntax needs to be used.
		/// </summary>
		RegexpFilter = 2,

		/// <summary>
		///     Not implemented yet.
		/// </summary>
		TimeFilter = 3
	}
}