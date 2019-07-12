using System;

namespace Tailviewer.PluginRepository
{
	public static class Bootstrapper
	{
		public static int Main(string[] args)
		{
			try
			{
				return App.Run(args);
			}
			catch (Exception e)
			{
				// We don't use log4net here because we might not have been able to load the assembly and therefore
				// would crash even harder if we were to throw here
				Console.WriteLine("Exiting due to unexpected exception: {0}", e);
				return -1;
			}
		}
	}
}