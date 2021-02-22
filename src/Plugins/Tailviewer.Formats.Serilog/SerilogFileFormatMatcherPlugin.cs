using Tailviewer.Plugins;

namespace Tailviewer.Formats.Serilog
{
	public sealed class SerilogFileFormatMatcherPlugin
		: ILogFileFormatMatcherPlugin
	{
		#region Implementation of ILogFileFormatMatcherPlugin

		public ILogFileFormatMatcher CreateMatcher(IServiceContainer services)
		{
			var registry = services.Retrieve<ILogFileFormatRepository>();
			return new SerilogFileFormatMatcher(registry);
		}

		#endregion
	}
}
