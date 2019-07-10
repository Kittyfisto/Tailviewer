using System;

namespace Tailviewer.PluginRegistry.Applications
{
	public static class ListPlugins
	{
		public static int Run(ListPluginsOptions options)
		{
			using (var repo = new PluginRepository())
			{
				foreach (var plugin in repo.FindAllPlugins())
				{
					Console.WriteLine("\t{0}", plugin);
				}

				return 0;
			}
		}
	}
}