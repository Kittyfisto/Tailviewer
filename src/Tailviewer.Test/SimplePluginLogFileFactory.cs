using System.Linq;
using System.Threading;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core;
using Tailviewer.Plugins;

namespace Tailviewer.Test
{
	public sealed class SimplePluginLogFileFactory
		: PluginLogFileFactory
	{
		public SimplePluginLogFileFactory(ITaskScheduler scheduler, params IFileFormatPlugin[] plugins)
			: base(CreateServiceContainer(scheduler), plugins.Select(x => new PluginWithDescription<IFileFormatPlugin>(x, null)))
		{}

		private static IServiceContainer CreateServiceContainer(ITaskScheduler scheduler)
		{
			var container = new ServiceContainer();
			container.RegisterInstance<ITaskScheduler>(scheduler);
			container.RegisterInstance<ILogFileFormatMatcher>(new SimpleLogFileFormatMatcher(LogFileFormats.GenericText));
			container.RegisterInstance<ITextLogFileParserPlugin>(new SimpleTextLogFileParserPlugin());
			return container;
		}
	}
}
