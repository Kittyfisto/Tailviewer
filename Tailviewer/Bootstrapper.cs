using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Metrolib;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Tailviewer
{
	public class Bootstrapper
		: AbstractBootstrapper
	{
		[STAThread]
		public static int Main(string[] args)
		{
			// The following code ensures that only one Tailviewer window is shown to the user.
			// The code tries to find out if an already running process can still be used
			// (after all the user might have started tailviewer again because it froze or because
			// its window is not showing, but the process still running.
			//
			// Once a decision has been made, either all other application instances are forcefully closed
			// or this process terminates peacefully.
			SingleApplicationHelper.IMutex mutex = null;
			try
			{
				mutex = SingleApplicationHelper.AcquireMutex();
				if (mutex == null)
				{
					Console.WriteLine("WARN: Unable to acquire mutex, another Tailviewer is already running!");

					var processes = SingleApplicationHelper.FindOtherTailviewers();
					Process primaryProcess;
					if (SingleApplicationHelper.ShouldTakeOver(processes, out primaryProcess))
					{
						Console.WriteLine(
							"INFO: Tailviewer with pid {0} has been determined to not be usable anymore",
							primaryProcess?.Id);

						SingleApplicationHelper.KillAllOtherInstances(processes);

						Console.WriteLine("INFO: Trying to take over mutex...");
						mutex = SingleApplicationHelper.AcquireMutex();
					}
					else
					{
						Console.WriteLine("INFO: Tailviewer with pid {0} has been determined to still be usable",
							primaryProcess?.Id);

						// I guess the already running process
						// is good enough for the user: We should
						// kill ourselves so we don't interfere with
						// it.
						//
						// But before we do that, we have to send the files
						// we were supposed to open to the other process.
						SingleApplicationHelper.OpenFile(primaryProcess, args);
						SingleApplicationHelper.BringToFront(primaryProcess);

						Console.WriteLine("INFO: Signing off...");

						return 0;
					}
				}

				SetupLoggers();
				return App.Start(mutex, args);
			}
			finally
			{
				mutex?.Dispose();
			}
		}

		/// <summary>
		/// Tests if there is another Tailviewer instance already running.
		/// If there is, tests if it reacts to input.
		/// If it does, then this process is exited immediately.
		/// If it does not, then the other process is killed and this process 
		/// </summary>
		private static void EnsureOneApplication()
		{
			throw new NotImplementedException();
		}

		private static void SetupLoggers()
		{
			var hierarchy = (Hierarchy) LogManager.GetRepository();

			var patternLayout = new PatternLayout
				{
					ConversionPattern = "%date [%thread] %-5level %logger - %message%newline"
				};
			patternLayout.ActivateOptions();

			var fileAppender = new RollingFileAppender
				{
					AppendToFile = false,
					File =
						Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Tailviewer",
						             "Tailviewer.log"),
					Layout = patternLayout,
					MaxSizeRollBackups = 20,
					MaximumFileSize = "1GB",
					RollingStyle = RollingFileAppender.RollingMode.Size,
					StaticLogFileName = false
				};
			fileAppender.ActivateOptions();
			hierarchy.Root.AddAppender(fileAppender);

			hierarchy.Root.Level = Level.Info;
			hierarchy.Configured = true;
		}
	}
}