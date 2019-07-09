namespace Tailviewer.PluginRegistry.Applications
{
	public static class AddPlugin
	{
		public static int Run(AddPluginOptions options)
		{
			using (var repo = new PluginRepository())
			{
				repo.AddPlugin(options.PluginFileName);

				return 0;
			}
		}
	}
}
