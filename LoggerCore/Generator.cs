using System;
using System.Threading;
using log4net;
using log4net.Core;

namespace LoggerCore
{
	/// <summary>
	///     Responsible for writing log messages to its given logger at random intervals.
	/// </summary>
	public sealed class Generator
		: IDisposable
	{
		private readonly ILog _log;
		private readonly LoggingEventTemplate[] _messages;
		private readonly Random _random;
		private readonly IPeriodicTask _task;
		private readonly ITaskScheduler _taskScheduler;
		private int _count;

		public Generator(ITaskScheduler taskScheduler, ILog log)
		{
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));
			if (log == null)
				throw new ArgumentNullException(nameof(log));

			_random = new Random();
			_taskScheduler = taskScheduler;
			_log = log;

			_messages = CreateMessages();

			_task = _taskScheduler.StartPeriodic(Update);
		}

		public void Dispose()
		{
			_taskScheduler.StopPeriodic(_task);
		}

		private LoggingEventTemplate[] CreateMessages()
		{
			return new[]
				{
					new LoggingEventTemplate(Level.Info, "Starting transaction..."),
					new LoggingEventTemplate(Level.Info, "Processing request #12345"),
					new LoggingEventTemplate(Level.Debug, "Current value: 3.14159"),
					new LoggingEventTemplate(Level.Warn, "Unable to parse '3231sass': Expected integer number"),
					new LoggingEventTemplate(Level.Error, "Caught unexpected exception: System.StackoverflowException"),
					new LoggingEventTemplate(Level.Fatal, "Wrong universe!"),
					new LoggingEventTemplate(Level.Info, @"Platform assembly: F:\Unity\Editor\Data\Managed\UnityEngine.dll (this message is harmless)"),
					new LoggingEventTemplate(Level.Debug, "Initialize engine version: 5.5.0f3 (38b4efef76f0)"),
					new LoggingEventTemplate(Level.Info, @"Setting StandaloneWindows v5.5.0 for Unity v5.5.0f3 to F:\Unity\Editor\Data\PlaybackEngines\windowsstandalonesupport"),
					new LoggingEventTemplate(Level.Info, "Unloading 38 Unused Serialized files (Serialized files now loaded: 0)"),
					new LoggingEventTemplate(Level.Info, "[00:00:01] Enlighten: Finished 1 Layout Systems job (0.00s execute, 0.00s integrate, 0.22s wallclock)"),
					new LoggingEventTemplate(Level.Debug, "Refreshing native plugins compatible for Editor in 0.45 ms, found 4 plugins."),
					new LoggingEventTemplate(Level.Info, "[00:00:07] Enlighten: Bake started."),
					new LoggingEventTemplate(Level.Info, "Packing sprites:"),
					new LoggingEventTemplate(Level.Info, "Preloading 2 native plugins for Editor in 0.13 ms.")
				};
		}

		private TimeSpan Update()
		{
			int index = _random.Next(0, _messages.Length - 1);
			LoggingEventTemplate template = _messages[index];
			if (template.Level == Level.Debug)
			{
				_log.Debug(template.Message);
			}
			else if (template.Level == Level.Info)
			{
				_log.Info(template.Message);
			}
			else if (template.Level == Level.Warn)
			{
				_log.Warn(template.Message);
			}
			else if (template.Level == Level.Error)
			{
				_log.Error(template.Message);
			}
			else if (template.Level == Level.Fatal)
			{
				_log.Fatal(template.Message);
			}

			++_count;

			TimeSpan waitTime = TimeSpan.FromMilliseconds(_random.Next(10, 1000));
			return waitTime;
		}

		public int Count
		{
			get { return _count; }
		}
	}
}