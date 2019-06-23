using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Archiver.Plugins
{
	public interface IPluginWithDescription<out T> where T : class, IPlugin
	{
		T Plugin { get; }
		IPluginDescription Description { get; }
	}
}