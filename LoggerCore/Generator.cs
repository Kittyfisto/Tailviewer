using System;
using System.Threading;
using System.Threading.Tasks;
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
				throw new ArgumentNullException("taskScheduler");
			if (log == null)
				throw new ArgumentNullException("log");

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
					new LoggingEventTemplate(Level.Fatal, "Wrong universe!")
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