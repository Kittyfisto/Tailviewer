using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Maintains a collection of well-known log file properties only applicable to text-based log files.
	/// </summary>
	public static class TextProperties
	{
		/// <summary>
		///     The number of lines the original source consists of.
		/// </summary>
		/// <remarks>
		///     <see cref="ILogFile"/> implementations which filter their log file must not touch this value and simply
		///     forward it as is.
		/// </remarks>
		public static readonly IReadOnlyPropertyDescriptor<long> LineCount;

		/// <summary>
		///     The maximum number of characters the greatest line in the source consists of.
		/// </summary>
		/// <remarks>
		///     <see cref="ILogFile"/> implementations which aggregate multiple log files should forward the largest
		///     value of their sources and forward that one.
		/// </remarks>
		public static readonly IReadOnlyPropertyDescriptor<int> MaxCharactersInLine;

		static TextProperties()
		{
			var category = "text";
			LineCount = new WellKnownReadOnlyProperty<long>(new []{category, "line_count"}, "Lines");
			MaxCharactersInLine = new WellKnownReadOnlyProperty<int>(new []{category, "max_characters_in_line"});
		}
	}
}