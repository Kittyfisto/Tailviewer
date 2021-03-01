using Tailviewer.Plugins;

namespace Tailviewer.LogLevelPlugin
{
	public sealed class MyCustomLogEntryParserPlugin
		: ILogEntryParserPlugin
	{
		#region Implementation of ILogEntryParserPlugin

		public ILogEntryParser CreateParser(IServiceContainer services, ILogFileFormat format)
		{
			// We want to make sure that we only influence those log files we actually
			// know the structure of. Therefore we check that the format of the log file
			// we're supposed to parse now IS the format our matcher has detected.
			if (format == MyLogFileFormatMatcherPlugin.MyCustomFormat)
				return new MyCustomLogLevelParser();

			// If it is a different format, then we don't do anything and instead return null
			// so that a different plugin OR Tailviewer itself may parse its contents.
			return null;
		}

		#endregion
	}
}