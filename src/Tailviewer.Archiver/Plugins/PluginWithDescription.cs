using Tailviewer.Api;
using Tailviewer.Archiver.Plugins.Description;

namespace Tailviewer.Archiver.Plugins
{
	public sealed class PluginWithDescription<T>
		: IPluginWithDescription<T>
		where T : class, IPlugin
	{
		public T Plugin { get; }
		public IPluginDescription Description { get; }

		public PluginWithDescription(T plugin, IPluginDescription description)
		{
			Plugin = plugin;
			Description = description;
		}
	}
}