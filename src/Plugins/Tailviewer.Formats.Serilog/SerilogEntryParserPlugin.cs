using System;
using System.Collections.Generic;
using Tailviewer.Plugins;

namespace Tailviewer.Formats.Serilog
{
	/// <summary>
	///     Plugin to allow parsing of serilog text files.
	/// </summary>
	public sealed class SerilogEntryParserPlugin
		: ILogEntryParserPlugin
	{
		#region Implementation of ITextLogFileParserPlugin

		IReadOnlyList<ILogFileFormat> ILogEntryParserPlugin.SupportedFormats
		{
			get { return new ILogFileFormat[0]; }
		}

		public ILogEntryParser CreateParser(IServiceContainer services, ILogFileFormat format)
		{
			var serilogFormat = format as SerilogFileFormat;
			if (serilogFormat == null)
				throw new ArgumentException($"Unsupported format: {format}");

			return serilogFormat.Parser;
		}

		#endregion
	}
}