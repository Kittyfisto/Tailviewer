using System;
using System.Reflection;
using log4net;
using Tailviewer.Plugins;

namespace Tailviewer.Formats.Serilog
{
	/// <summary>
	///     Plugin to allow adding serilog formats to Tailviewer.
	/// </summary>
	public sealed class SerilogFileFormatCreatorPlugin
		: ICustomLogFileFormatCreatorPlugin
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public string FormatName
		{
			get { return "Serilog"; }
		}

		public bool TryCreate(IServiceContainer serviceContainer,
		                      ICustomLogFileFormat format,
		                      out ILogFileFormat logFileFormat)
		{
			try
			{
				logFileFormat = new SerilogFileFormat(format.Name, format.Format, format.Encoding);
				return true;
			}
			catch (Exception e)
			{
				Log.WarnFormat("Unable to create serilog format from '{0}': {1}", format, e);
				logFileFormat = null;
				return false;
			}
		}
	}
}