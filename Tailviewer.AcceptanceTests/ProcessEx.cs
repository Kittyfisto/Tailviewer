using System;
using System.Diagnostics;

namespace Tailviewer.AcceptanceTests
{
	public sealed class ProcessEx
		: IDisposable
	{
		private readonly Process _process;
		private readonly string _id;

		private ProcessEx(string id, Process process)
		{
			_id = id;
			_process = process;
			_process.BeginOutputReadLine();
			_process.OutputDataReceived += ProcessOnOutputDataReceived;
		}

		private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args)
		{
			Console.WriteLine("{0}: {1}", _id, args.Data);
		}

		public static ProcessEx Start(string id, string fileName)
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo(fileName)
				{
					UseShellExecute = false,
					RedirectStandardOutput = true
				}
			};
			process.Start();
			return new ProcessEx(id, process);
		}

		public bool HasExited => _process.HasExited;

		public int ExitCode => _process.ExitCode;

		public bool WaitForExit(int milliseconds)
		{
			return _process.WaitForExit(milliseconds);
		}

		public void Dispose()
		{
			try
			{
				if (!_process.HasExited)
					_process?.Kill();
			}
			catch (Exception)
			{}
			_process?.Dispose();
		}
	}
}