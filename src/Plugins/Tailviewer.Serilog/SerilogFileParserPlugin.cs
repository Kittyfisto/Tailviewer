using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Serilog
{
	/// <summary>
	///     Plugin to allow parsing of serilog text files.
	/// </summary>
	public sealed class SerilogFileParserPlugin
		: ITextLogFileParserPlugin
	{
		#region Implementation of ITextLogFileParserPlugin

		IReadOnlyList<ILogFileFormat> ITextLogFileParserPlugin.SupportedFormats
		{
			get { return new ILogFileFormat[0]; }
		}

		public ITextLogFileParser CreateParser(IServiceContainer services, ILogFileFormat format)
		{
			var serilogFormat = format as SerilogFileFormat;
			if (serilogFormat == null)
				throw new ArgumentException($"Unsupported format: {format}");

			return serilogFormat.Parser;
		}

		#endregion
	}
}