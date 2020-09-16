using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Serilog
{
	public sealed class SerilogFileFormatMatcherPlugin
		: ILogFileFormatMatcherPlugin
	{
		#region Implementation of ILogFileFormatMatcherPlugin

		public ILogFileFormatMatcher CreateMatcher(IServiceContainer services)
		{
			var registry = services.Retrieve<ILogFileFormatRegistry>();
			return new SerilogFileFormatMatcher(registry);
		}

		#endregion
	}
}
