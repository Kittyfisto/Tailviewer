using System;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Tailviewer.Test
{
	public sealed class Logger
		: IDisposable
	{
		private readonly Hierarchy _hierarchy;

		public Logger(string fileName)
		{
			_hierarchy = (Hierarchy) LogManager.GetRepository();

			var patternLayout = new PatternLayout
				{
					ConversionPattern = "%date [%thread] %-5level %logger - %message%newline"
				};
			patternLayout.ActivateOptions();

			var roller = new FileAppender
				{
					AppendToFile = false,
					File = fileName,
					Layout = patternLayout,
				};
			roller.ActivateOptions();
			_hierarchy.Root.AddAppender(roller);

			var memory = new MemoryAppender();
			memory.ActivateOptions();
			_hierarchy.Root.AddAppender(memory);

			_hierarchy.Root.Level = Level.Info;
			_hierarchy.Configured = true;
		}

		public void Dispose()
		{
			_hierarchy.Clear();
		}
	}
}