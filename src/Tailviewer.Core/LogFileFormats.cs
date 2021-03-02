using System.Text;
using Tailviewer.Api;

namespace Tailviewer.Core
{
	/// <summary>
	///     Maintains a collection of well-known log file formats that tailviewer can detect out-of-the-box.
	/// </summary>
	/// <remarks>
	///     Plugins are encouraged to implement their own <see cref="ILogFileFormatMatcherPlugin" /> in order
	///     to detect more formats.
	/// </remarks>
	public static class LogFileFormats
	{
		/// <summary>
		///     This format is used for any log file unless it has been matched to another, more specific, format.
		/// </summary>
		public static readonly ILogFileFormat GenericText;

		/// <summary>
		///     The log file is an ordinary comma separated value file.
		/// </summary>
		public static readonly ILogFileFormat Csv;

		/// <summary>
		///     The Common Log Format, also known as the NCSA Common log format
		///     is a standardized text file format used by web servers when generating server log files.
		///     <see href="https://en.wikipedia.org/wiki/Common_Log_Format" /> for details.
		/// </summary>
		public static readonly ILogFileFormat CommonLogFormat;

		/// <summary>
		///     Extended Log Format (ELF) is a standardized text file format, like Common Log Format (CLF),
		///     that is used by web servers when generating log files, but ELF files provide more information and flexibility.
		///     <see href="https://en.wikipedia.org/wiki/Extended_Log_Format" /> for details.
		/// </summary>
		public static readonly ILogFileFormat ExtendedLogFormat;

		static LogFileFormats()
		{
			GenericText = new TextLogFileFormat("Generic Text");
			Csv = new TextLogFileFormat("CSV");
			CommonLogFormat = new TextLogFileFormat("Common Log Format",
			                                        "The Common Log Format, also known as the NCSA Common log format is a standardized text file format used by web servers when generating server log files.",
			                                        Encoding.ASCII);
			ExtendedLogFormat = new TextLogFileFormat("Extended Log Format",
			                                          "Extended Log Format (ELF) is a standardized text file format, like Common Log Format (CLF), that is used by web servers when generating log files, but ELF files provide more information and flexibility.",
			                                          Encoding.ASCII);
		}
	}
}