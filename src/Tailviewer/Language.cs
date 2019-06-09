using System.Diagnostics.Contracts;

namespace Tailviewer
{
	public static class Language
	{
		[Pure]
		public static string Pluralize(string @base, int count)
		{
			return count == 1
				? @base
				: @base + 's';
		}

		/// <summary>
		/// Builds a string (with proper pluralization, if necessary) such as:
		/// 1 file
		/// 2 files
		/// etc...
		/// </summary>
		/// <param name="base"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		[Pure]
		public static string Count(string @base, int count)
		{
			return string.Format("{0} {1}", count, Pluralize(@base, count));
		}
	}
}
