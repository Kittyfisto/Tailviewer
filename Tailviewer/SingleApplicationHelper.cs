using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Tailviewer
{
	/// <summary>
	/// This class is responsible for:
	/// - Ensuring that the user only sees one process (window) of this application
	/// - Ensuring that the other process is told to open additional files
	/// - Ensuring that existing process is killed if it doesn't react anymore
	/// </summary>
	public static class SingleApplicationHelper
	{
		/// <summary>
		/// Tries to acquire an exclude mutex.
		/// </summary>
		/// <returns>The acquired mutex or null if another process holds the mutex</returns>
		public static IDisposable AcquireMutex()
		{
			bool acquiredLock;
			var mutex = new Mutex(true, "Kittyfisto.Tailviewer", out acquiredLock);
			try
			{
				if (!acquiredLock)
				{
					mutex.Dispose();
					return null;
				}

				return mutex;
			}
			catch (Exception)
			{
				mutex.Dispose();
				throw;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<Process> FindOtherTailviewers()
		{
			var processes = Process.GetProcessesByName("Tailviewer").ToList();
			using (var current = Process.GetCurrentProcess())
			{
				processes.Remove(current);
			}
			return processes;
		}

		/// <summary>
		/// Tests if this application should take over work in favour of the already
		/// running application(s).
		/// </summary>
		/// <param name="processes"></param>
		/// <param name="primaryProcess"></param>
		/// <returns>True when this application should take over, false otherwise</returns>
		public static bool ShouldTakeOver(IEnumerable<Process> processes, out Process primaryProcess)
		{
			primaryProcess = processes.FirstOrDefault();
			// TODO: Implement
			return false;
		}

		/// <summary>
		/// Kills all other Tailviewer processes, if there are any.
		/// Does nothing otherwise.
		/// </summary>
		/// <param name="processes"></param>
		public static void KillAllOtherInstances(IEnumerable<Process> processes)
		{
			foreach (var process in processes)
			{
				TryKill(process);
			}
		}

		private static void TryKill(Process process)
		{
			try
			{
				Console.WriteLine("Killing process with pid {0}...", process.Id);

				process.Kill();
			}
			catch (Exception e)
			{
				Console.WriteLine("Caught unexpected exception while trying to kill process {0}: {1}",
					process.Id,
					e);
			}
		}

		/// <summary>
		/// Forwards the given list of 
		/// </summary>
		/// <param name="primaryProcess"></param>
		/// <param name="args">The commandline arguments as given to Main()</param>
		public static void ForwardFilesTo(Process primaryProcess, string[] args)
		{
			try
			{
				var arguments = ArgumentParser.TryParse(args);
				if (arguments.FilesToOpen.Length > 0)
				{
					
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Caught unexpected exception: {0}", e);
			}
		}

		/// <summary>
		/// Ensures that the primary window of the given process is brought to the front.
		/// Doesn't do anything when it already is.
		/// Doesn't do anything when the given process doesn't exist (anymore)
		/// </summary>
		/// <param name="primaryProcess"></param>
		public static void BringToFront(Process primaryProcess)
		{
			try
			{
				var windowHandle = primaryProcess.MainWindowHandle;

				Console.WriteLine("Ensuring mainwindow {0} of pid {1} is visible",
					windowHandle,
					primaryProcess.Id
				);

				if (windowHandle != IntPtr.Zero)
				{
					SetForegroundWindow(windowHandle);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Caught unexpected exception: {0}", e);
			}
		}

		[DllImport("USER32.DLL")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);
	}
}