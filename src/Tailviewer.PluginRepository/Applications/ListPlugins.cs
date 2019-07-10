using System;

namespace Tailviewer.PluginRepository.Applications
{
	public sealed class ListPlugins
		: IApplication<ListPluginsOptions>
	{
		public int Run(PluginRepository repository, ListPluginsOptions options)
		{
			foreach (var plugin in repository.FindAllPlugins())
			{
				Console.WriteLine("\t{0}", plugin);
			}

			return 0;
		}
	}
}