using System.Text;

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
		///    Whether or not the text log file *may* be interpreted by Tailviewer as a multi-line document.
		/// </summary>
		/// <remarks>
		///    In multi-line log files, log entries may span multiple lines.
		///    In single-line log files, every line is its own log entry, always.
		/// </remarks>
		public static readonly IReadOnlyPropertyDescriptor<bool> AllowsMultiline;

		/// <summary>
		///     Set to true when the text file starts with a BOM which told tailviewer about the specific encoding used.
		///     Set to false when the text file doesn't start with a (recognizable) BOM.
		///     Set to null when the text is is empty, cannot be read, etc...
		/// </summary>
		public static readonly IReadOnlyPropertyDescriptor<bool?> ByteOrderMark;

		/// <summary>
		///     The <see cref="System.Text.Encoding"/> of a text file, as detected by Tailviewer.
		/// </summary>
		/// <remarks>
		///     Since this is merely an educated guess, it's possible that Tailviewer get's it wrong in which case
		///     it can be overwritten by setting the <see cref="OverwrittenEncoding"/> property.
		/// </remarks>
		public static readonly IReadOnlyPropertyDescriptor<Encoding> AutoDetectedEncoding;

		/// <summary>
		///     The <see cref="System.Text.Encoding"/> of a text file as overwritten by the user.
		/// </summary>
		/// <remarks>
		///     
		/// </remarks>
		public static readonly IPropertyDescriptor<Encoding> OverwrittenEncoding;

		/// <summary>
		///     The <see cref="System.Text.Encoding"/> which Tailviewer currently uses to to decode the file.
		/// </summary>
		public static readonly IReadOnlyPropertyDescriptor<Encoding> Encoding;

		static TextProperties()
		{
			var category = "text";
			LineCount = new WellKnownReadOnlyProperty<long>(new []{category, "line_count"});
			MaxCharactersInLine = new WellKnownReadOnlyProperty<int>(new []{category, "max_characters_in_line"});
			AllowsMultiline = new WellKnownProperty<bool>(new[] {category, "allows_multiline"});
			ByteOrderMark = new WellKnownReadOnlyProperty<bool?>(new[] {category, "byte_order_mark"});
			AutoDetectedEncoding = new WellKnownReadOnlyProperty<Encoding>(new []{category, "auto_detected_encoding"});
			OverwrittenEncoding = new WellKnownProperty<Encoding>(new []{category, "overwritten_encoding"});
			Encoding = new WellKnownReadOnlyProperty<Encoding>(new []{category, "encoding"});
		}
	}
}