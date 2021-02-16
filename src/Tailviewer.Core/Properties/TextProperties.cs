namespace Tailviewer.Core.Properties
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
		///     <see cref="ILogSource"/> implementations which filter their log file must not touch this value and simply
		///     forward it as is.
		/// </remarks>
		public static readonly IReadOnlyPropertyDescriptor<long> LineCount;

		/// <summary>
		///     The maximum number of characters the greatest line in the source consists of.
		/// </summary>
		/// <remarks>
		///     <see cref="ILogSource"/> implementations which aggregate multiple log files should forward the largest
		///     value of their sources and forward that one.
		/// </remarks>
		public static readonly IReadOnlyPropertyDescriptor<int> MaxCharactersInLine;

		/// <summary>
		///    Whether or not the text log file is a multi-line log file.
		/// </summary>
		/// <remarks>
		///    In multi-line log files, log entries may span multiple lines.
		///    In single-line log files, every line is its own log entry, always.
		/// </remarks>
		public static readonly IReadOnlyPropertyDescriptor<bool> IsMultiline;

		static TextProperties()
		{
			var category = "text";
			LineCount = new WellKnownReadOnlyProperty<long>(new []{category, "line_count"});
			MaxCharactersInLine = new WellKnownReadOnlyProperty<int>(new []{category, "max_characters_in_line"});
			IsMultiline = new WellKnownProperty<bool>(new[] {category, "is_multiline"});
		}
	}
}