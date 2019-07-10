using System;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Tailviewer.PluginRepository
{
	public static class Bootstrapper
	{
		public static int Main(string[] args)
		{
			try
			{
				SetupConsoleLogger();

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

		private static void SetupConsoleLogger()
		{
			var hierarchy = (Hierarchy) LogManager.GetRepository();

			var consoleLayout = new PatternLayout
			{
				ConversionPattern = "%date %-5level - %message%newline"
			};
			consoleLayout.ActivateOptions();
			var consoleAppender = new ConsoleAppender
			{
				Layout = consoleLayout
			};
			consoleAppender.ActivateOptions();
			hierarchy.Root.AddAppender(consoleAppender);

			hierarchy.Root.Level = Level.Info;
			hierarchy.Configured = true;
		}
	}
}