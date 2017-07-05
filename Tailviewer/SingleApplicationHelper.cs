using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

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
		///     The interface for a system-wide-exclusive mutex.
		///     The mutex is acquired until it's being disposed of.
		/// </summary>
		public interface IMutex
			: IDisposable
		{
			/// <summary>
			/// The given listener is notified when other applications send requests
			/// to the application actually holding the mutex.
			/// </summary>
			/// <param name="listener"></param>
			void SetListener(IMessageListener listener);
		}

		/// <summary>
		/// 
		/// </summary>
		public interface IMessageListener
		{
			/// <summary>
			/// This method is called when another application requests that the data source
			/// with the given uri should be either shown (if it's already present) or
			/// opened and then shown.
			/// </summary>
			/// <param name="dataSourceUri"></param>
			void OnOpenDataSource(string dataSourceUri);
		}

		sealed class ExclusiveMutex
			: IMutex
		{
			private readonly Mutex _mutex;
			private readonly object _syncRoot;
			private readonly Thread _thread;
			private bool _isDisposed;
			private IMessageListener _listener;

			private ExclusiveMutex(Mutex mutex)
			{
				_mutex = mutex;
				_syncRoot = new object();
				_thread = new Thread(Read)
				{
					IsBackground = true
				};
				_thread.Start();
			}

			private void Read()
			{
				while (!_isDisposed)
				{
					CheckFiles();
					Thread.Sleep(TimeSpan.FromMilliseconds(500));
				}
			}

			private void CheckFiles()
			{
				var fname = Path.Combine(TheFuckingDir, MessageType.OpenFile.ToString());
				try
				{
					if (File.Exists(fname))
					{
						try
						{
							var messageData = File.ReadAllBytes(fname);
							Dispatch(MessageType.OpenFile, messageData);
						}
						finally
						{
							File.Delete(fname);
						}
					}
				}
				catch (Exception e)
				{
				}
			}

			public static ExclusiveMutex TryAcquire()
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

					return new ExclusiveMutex(mutex);
				}
				catch (Exception)
				{
					mutex.Dispose();
					throw;
				}
			}

			public void Dispose()
			{
				_mutex.Dispose();
				_isDisposed = true;
			}

			public void SetListener(IMessageListener listener)
			{
				lock (_syncRoot)
				{
					_listener = listener;

					SendMessage(Process.GetCurrentProcess().MainWindowHandle,
						MessageType.OpenFile,
						new byte[7]);
				}
			}

			private void Dispatch(MessageType message, byte[] messageData)
			{
				switch (message)
				{
					case MessageType.OpenFile:
						var file = Encoding.UTF8.GetString(messageData);
						OnOpenFile(file);
						break;
				}
			}

			/// <summary>
			///     This method is called when another process has instructed us to open and/or show
			///     the given data source.
			/// </summary>
			/// <param name="file"></param>
			private void OnOpenFile(string file)
			{
				_listener?.OnOpenDataSource(file);
			}
			
			private enum MessageType
			{
				OpenFile = 0xAFFE
			}

			/// <summary>
			///     Tells the given tailviewer process to open and/or show a data source
			///     with the given uri.
			/// </summary>
			/// <param name="process"></param>
			/// <param name="filename"></param>
			/// <returns></returns>
			public static void OpenFile(Process process, string filename)
			{
				var data = Encoding.UTF8.GetBytes(filename);
				SendMessage(process.MainWindowHandle,
					MessageType.OpenFile,
					data);
			}

			/// <summary>
			///     Sends a message to the given window and blocks for up to the given amount of time.
			///     If the message hasn't fully been processed after the timeout elapses, the method
			///     returns false.
			/// </summary>
			/// <param name="windowHandle"></param>
			/// <param name="messageType"></param>
			/// <param name="data"></param>
			private static void SendMessage(IntPtr windowHandle,
				MessageType messageType, byte[] data)
			{
				if (!Directory.Exists(TheFuckingDir))
				{
					Directory.CreateDirectory(TheFuckingDir);
				}

				var fname = Path.Combine(TheFuckingDir, messageType.ToString());
				using (var stream = File.OpenWrite(fname))
				using (var writer = new BinaryWriter(stream))
				{
					writer.Write(data);
				}
			}

			private static readonly string TheFuckingDir = Path.Combine(Constants.AppDataLocalFolder, "tmp", "open");
			
		}

		/// <summary>
		/// Tries to acquire an exclude mutex.
		/// </summary>
		/// <returns>The acquired mutex or null if another process holds the mutex</returns>
		public static IMutex AcquireMutex()
		{
			return ExclusiveMutex.TryAcquire();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<Process> FindOtherTailviewers()
		{
			var processes = new List<Process>();
			processes.AddRange(Process.GetProcessesByName("Tailviewer"));
			processes.AddRange(Process.GetProcessesByName("Tailviewer.vshost"));
			using (var current = Process.GetCurrentProcess())
			{
				processes.RemoveAll(x => x.Id == current.Id);
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
		/// Instructs the given tailviewer process to open and/or show the given data source
		/// </summary>
		/// <param name="primaryProcess"></param>
		/// <param name="args">The commandline arguments as given to Main()</param>
		public static void OpenFile(Process primaryProcess, string[] args)
		{
			try
			{
				var windowHandle = primaryProcess.MainWindowHandle;
				var arguments = ArgumentParser.TryParse(args);
				if (windowHandle != IntPtr.Zero && arguments.FileToOpen != null)
				{
					ExclusiveMutex.OpenFile(primaryProcess, arguments.FileToOpen);
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

		#region IPC

		#endregion
	}
}