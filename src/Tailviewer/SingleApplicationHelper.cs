using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MMQ;

namespace Tailviewer
{
	/// <summary>
	///     This class is responsible for:
	///     - Ensuring that the user only sees one process (window) of this application
	///     - Ensuring that the other process is told to open additional files
	///     - Ensuring that existing process is killed if it doesn't react anymore
	/// </summary>
	public static class SingleApplicationHelper
	{
		/// <summary>
		///     Tries to acquire an exclude mutex.
		/// </summary>	
		/// <returns>The acquired mutex or null if another process holds the mutex</returns>
		public static IMutex AcquireMutex()
		{
			return AcquireMutex(TimeSpan.Zero);
		}

		/// <summary>
		///     Tries to acquire an exclude mutex until:
		/// - an exclusive mutex was acquired
		/// - the timeout elapsed
		/// </summary>
		/// <returns>The acquired mutex or null if another process holds the mutex</returns>
		public static IMutex AcquireMutex(TimeSpan timeout)
		{
			DateTime start = DateTime.Now;
			TimeSpan elapsed;

			do
			{
				var mutex = ExclusiveMutex.TryAcquire();
				if (mutex != null)
					return mutex;

				Thread.Sleep(TimeSpan.FromMilliseconds(1));

				elapsed = DateTime.Now - start;
			} while (elapsed < timeout);

			return null;
		}

		/// <summary>
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
		///     Tests if this application should take over work in favour of the already
		///     running application(s).
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
		///     Kills all other Tailviewer processes, if there are any.
		///     Does nothing otherwise.
		/// </summary>
		/// <param name="processes"></param>
		public static void KillAllOtherInstances(IEnumerable<Process> processes)
		{
			foreach (var process in processes)
				TryKill(process);
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
		///     Instructs the given tailviewer process to open and/or show the given data source
		/// </summary>
		/// <param name="args">The commandline arguments as given to Main()</param>
		public static void OpenFile(string[] args)
		{
			try
			{
				var arguments = ArgumentParser.TryParse(args);
				if (arguments.FileToOpen != null)
					ExclusiveMutex.OpenFile(arguments.FileToOpen);
			}
			catch (Exception e)
			{
				Console.WriteLine("Caught unexpected exception: {0}", e);
			}
		}

		/// <summary>
		///     Ensures that the mainwindow of the primary tailviewer is brought to the front.
		///     Doesn't do anything when it already is.
		///     Doesn't do anything when the given process doesn't exist (anymore)
		/// </summary>
		public static void BringToFront()
		{
			try
			{
				ExclusiveMutex.ShowMainWindow();
			}
			catch (Exception e)
			{
				Console.WriteLine("Caught unexpected exception: {0}", e);
			}
		}
		
		/// <summary>
		///     The interface for a system-wide-exclusive mutex.
		///     The mutex is acquired until it's being disposed of.
		/// </summary>
		public interface IMutex
			: IDisposable
		{
			/// <summary>
			///     The given listener is notified when other applications send requests
			///     to the application actually holding the mutex.
			/// </summary>
			/// <param name="listener"></param>
			void SetListener(IMessageListener listener);
		}

		/// <summary>
		/// </summary>
		public interface IMessageListener
		{
			/// <summary>
			/// 
			/// </summary>
			void OnShowMainwindow();

			/// <summary>
			///     This method is called when another application requests that the data source
			///     with the given uri should be either shown (if it's already present) or
			///     opened and then shown.
			/// </summary>
			/// <param name="dataSourceUri"></param>
			void OnOpenDataSource(string dataSourceUri);
		}

		/// <summary>
		///     Provides exclusive access to a machine-wide resource.
		///     Calling <see cref="TryAcquire" /> twice fails for as long as
		///     the previously created mutex has neither been disposed of
		///     nor its process exited.
		/// </summary>
		sealed class ExclusiveMutex
			: IMutex
		{
			private const string QueueName = "Kittyfisto.Tailviewer.SimpleIPC";

			private readonly IMemoryMappedQueueConsumer _consumer;
			private readonly IMemoryMappedQueue _queue;
			private readonly object _syncRoot;
			private readonly Thread _thread;
			private bool _isDisposed;
			private IMessageListener _listener;

			public static ExclusiveMutex TryAcquire()
			{
				IMemoryMappedQueue queue = null;
				try
				{
					queue = MemoryMappedQueue.Create(QueueName);
					return new ExclusiveMutex(queue);
				}
				catch (Exception)
				{
					queue?.Dispose();
					return null;
				}
			}

			private ExclusiveMutex(IMemoryMappedQueue queue)
			{
				_queue = queue;
				_consumer = queue.CreateConsumer();
				_syncRoot = new object();
				_thread = new Thread(Read)
				{
					IsBackground = true
				};
				_thread.Start();
			}

			public void Dispose()
			{
				_consumer.Dispose();
				_queue.Dispose();
				_isDisposed = true;
			}

			public void SetListener(IMessageListener listener)
			{
				lock (_syncRoot)
				{
					_listener = listener;
				}
			}

			private void Read()
			{
				while (!_isDisposed)
					try
					{
						if (_consumer.TryDequeue(out var message))
							Dispatch(message);
					}
					catch (ObjectDisposedException)
					{
						break;
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
						Thread.Sleep(TimeSpan.FromMilliseconds(500));
					}
			}

			private void Dispatch(byte[] message)
			{
				using (var stream = new MemoryStream(message))
				using (var reader = new BinaryReader(stream))
				{
					var id = (MessageType) reader.ReadByte();
					switch (id)
					{
						case MessageType.ShowMainwindow:
							_listener?.OnShowMainwindow();
							break;

						case MessageType.OpenFile:
							var file = Encoding.UTF8.GetString(reader.ReadBytes((int) (message.Length - stream.Position)));
							_listener?.OnOpenDataSource(file);
							break;

						default:
							Console.WriteLine("WARN: Unknown message id: {0}", id);
							break;
					}
				}
			}

			/// <summary>
			///     Ensures that the primary tailviewer application displays the given data source.
			/// </summary>
			/// <param name="filename"></param>
			/// <returns></returns>
			public static void OpenFile(string filename)
			{
				var data = Encoding.UTF8.GetBytes(filename);
				SendMessage(MessageType.OpenFile, data);
			}

			/// <summary>
			/// Ensures that the primary tailviewer window is visible to the user.
			/// </summary>
			public static void ShowMainWindow()
			{
				SendMessage(MessageType.ShowMainwindow);
			}

			/// <summary>
			///     Sends a message to the given window and blocks for up to the given amount of time.
			///     If the message hasn't fully been processed after the timeout elapses, the method
			///     returns false.
			/// </summary>
			/// <param name="messageType"></param>
			/// <param name="data"></param>
			private static void SendMessage(MessageType messageType, byte[] data = null)
			{
				using (var producer = MemoryMappedQueue.CreateProducer(QueueName))
				using (var stream = new MemoryStream())
				using (var writer = new BinaryWriter(stream))
				{
					writer.Write((byte)messageType);
					if (data != null)
						writer.Write(data);
					writer.Flush();

					producer.Enqueue(stream.ToArray());
				}
			}

			private enum MessageType : byte
			{
				/// <summary>
				///     When called, ensures that the main window is visible to the user:
				///     - It's restored if minimized
				///     - It's brought to the front of all windows
				///     - It's moved to be fully within the available desktop area (in case it's been moved offscreen)
				/// </summary>
				ShowMainwindow = 1,

				/// <summary>
				///     Ensures that tailviewer displays the given data source to the user.
				///     If no such data source has been opened, it will be opened.
				/// </summary>
				OpenFile = 2
			}
		}
	}
}