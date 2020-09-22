using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Serilog
{
	/// <summary>
	///     Plugin to allow adding serilog formats to Tailviewer.
	/// </summary>
	public sealed class SerilogFileFormatCreatorPlugin
		: ILogFileFormatCreatorPlugin
	{
		public string FormatName
		{
			get { return "Serilog"; }
		}

		public ILogFileFormat Create(ICustomLogFileFormat format)
		{
			return new SerilogFileFormat(format.Name, format.Format, format.Encoding);
		}
	}
}