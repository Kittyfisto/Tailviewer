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

		public ILogEntryParser CreateParser(IServiceContainer services, ILogFileFormat format)
		{
			var serilogFormat = format as SerilogFileFormat;
			if (serilogFormat == null)
				return null;

			return serilogFormat.Parser;
		}

		#endregion
	}
}