using System;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Tailviewer.Tests
{
	/// <summary>
	///     Configures log4net to log into a text file on disk.
	/// </summary>
	public sealed class FileLogger
		: IDisposable
	{
		private readonly Hierarchy _hierarchy;

		public FileLogger(string fileName)
		{
			_hierarchy = (Hierarchy) LogManager.GetRepository();

			var patternLayout = new PatternLayout
			{
				ConversionPattern = "%date [%thread] %-5level %logger - %message%newline"
			};
			patternLayout.ActivateOptions();

			var roller = new FileAppender
			{
				AppendToFile = true,
				File = fileName,
				Layout = patternLayout
			};
			roller.ActivateOptions();
			_hierarchy.Root.AddAppender(roller);

			_hierarchy.Root.Level = Level.Info;
			_hierarchy.Configured = true;
		}

		public void Dispose()
		{
			_hierarchy.Clear();
		}

		/// <summary>
		///     Configures the logger for that specific class to log with that level.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="level"></param>
		public void ConfigureLoggerOf<T>(Level level)
		{
			var type = typeof(T);
			var logger = (Logger) _hierarchy.GetLogger(type.FullName);
			logger.Level = level;
		}
	}
}