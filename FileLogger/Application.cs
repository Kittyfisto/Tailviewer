using System;
using System.IO;
using LoggerCore;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace FileLogger
{
	internal class Application
		: LoggerApplication
	{
		private Hierarchy _hierarchy;

		public Application()
		{
			SetupLog4Net();
		}

		private void SetupLog4Net()
		{
			var fileName = Path.Combine(Directory.GetCurrentDirectory(), @"..\Live\FileLogger.txt");
			Console.WriteLine("Logging to {0}", fileName);

			_hierarchy = (Hierarchy) LogManager.GetRepository();

			var patternLayout = new PatternLayout
				{
					ConversionPattern = "%date [%thread] %-5level %logger - %message%newline"
				};
			patternLayout.ActivateOptions();

			var roller = new FileAppender
				{
					AppendToFile = true,
					File = @"..\Live\FileLogger.txt",
					Layout = patternLayout,
				};
			roller.ActivateOptions();
			_hierarchy.Root.AddAppender(roller);

			_hierarchy.Root.Level = Level.Debug;
			_hierarchy.Configured = true;
		}

		private static int Main(string[] args)
		{
			return Run<Application>();
		}
	}
}