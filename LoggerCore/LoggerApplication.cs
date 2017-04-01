using System;
using System.Threading;
using log4net;

namespace LoggerCore
{
	public abstract class LoggerApplication
		: IDisposable
	{
		private readonly DefaultTaskScheduler _taskScheduler;
		private Generator _generator;

		protected LoggerApplication()
		{
			_taskScheduler = new DefaultTaskScheduler();
		}

		private void Start()
		{
			var logger = LogManager.GetLogger("SampleLogger");
			_generator = new Generator(_taskScheduler, logger);
		}

		public static int Run<T>() where T : LoggerApplication, new()
		{
			try
			{
				using (var app = new T())
				{
					app.Start();

					Console.WriteLine("Press escape to stop the logger...");

					while (true)
					{
						if (Console.ReadKey().Key == ConsoleKey.Escape)
						{
							Console.WriteLine("Stopping...");
							break;
						}
					}
				}

				return 0;
			}
			catch (Exception e)
			{
				Console.WriteLine("Caught unexpected exception: {0}", e);
				return -1;
			}
		}

		public virtual void Dispose()
		{
			if (_generator != null)
			{
				_generator.Dispose();
				Console.WriteLine("A total of {0} events have been logged", _generator.Count);
			}

			_taskScheduler.Dispose();
		}
	}
}