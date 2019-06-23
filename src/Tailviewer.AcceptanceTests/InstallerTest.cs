using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace Tailviewer.AcceptanceTests
{
	[TestFixture]
	public sealed class InstallerTest
		: SystemtestBase
	{
		private string _installationPath;

		[SetUp]
		public void Setup()
		{
			// Tests which execute Tailviewer.exe interefere with each other:
			// We introduce some timeout in between those tests so they are more likely
			// to succeed (until a proper fix has been implemented).
			Thread.Sleep(TimeSpan.FromMilliseconds(500));

			var dir = TestContext.CurrentContext.TestDirectory;
			var testName = TestContext.CurrentContext.Test.Name;
			_installationPath = Path.Combine(dir, testName);
		}

		[Test]
		[Description("Verifies that the graphical installer can start and doesn't exit on its own - also verifies that the log doesn't contain any errors")]
		public void TestRunGraphicalInstaller()
		{
			// We first try to run the graphical installer.
			// If there's a serious problem, the process will exit on its own and the test will fail.
			StartInstaller();

			// However there might be less serious problems which should've then been logged:
			var logFileName = Path.Combine(Constants.AppDataLocalFolder, "Installation.log");
			var logFile = ReadFile(logFileName);
			if (logFile.IndexOf("error", StringComparison.InvariantCultureIgnoreCase) != -1)
			{
				Console.WriteLine(logFile);
				Assert.Fail("Expected installer log to not contain any errors");
			}
		}

		private string ReadFile(string logFileName)
		{
			using (var stream = File.Open(logFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		[Test]
		[Description(
			"Verifies that a fresh install into a completely empty directory yields a runnable tailviewer application")]
		public void TestFreshInstall()
		{
			Delete(_installationPath);
			InstallInto(_installationPath);
			Execute(_installationPath);
		}

		[Test]
		public void TestOverwriteInstall()
		{
			Delete(_installationPath);
			InstallInto(_installationPath);
			InstallInto(_installationPath);
			Execute(_installationPath);
		}

		[Test]
		public void TestInstallWhileApplicationIsRunning()
		{
			Delete(_installationPath);
			InstallInto(_installationPath);
			using (var process = Start(_installationPath))
			{
				try
				{
					process.HasExited.Should().BeFalse();
					InstallInto(_installationPath);
					process.HasExited.Should()
					       .BeTrue("becuase the installer should've automatically killed TV prior to installing");
				}
				finally
				{
					TryKill(process);
				}
			}
		}

		private void TryKill(Process process)
		{
			try
			{
				process.Kill();
			}
			catch (Exception)
			{}
		}

		private void Delete(string installationPath)
		{
			if (Directory.Exists(installationPath))
			{
				var files = Directory.EnumerateFiles(installationPath, "*", SearchOption.AllDirectories).ToList();
				foreach (var file in files)
				{
					File.Delete(file);
				}
				Directory.Delete(installationPath, true);
			}
		}

		private void StartInstaller()
		{
			var startInfo = new ProcessStartInfo(InstallerPath);
			var process = Process.Start(startInfo);
			try
			{
				if (process.WaitForExit(5000))
				{
					Assert.Fail("Expected process with id {0} to not terminate on its own, but it did with exitCode {1}",
						process.Id,
						process.ExitCode);
				}
			}
			finally
			{
				process.Kill();
			}
		}

		private static void Execute(string installationPath)
		{
			var path = Path.Combine(installationPath, "Tailviewer.exe");
			var process = Process.Start(path);
			const string reason = "because the application shouldn't exit within the first five seconds";
			process.WaitForExit(5000);

			if (process.HasExited)
			{
				PrintLogFile();
				process.HasExited.Should().BeFalse(reason);
			}
			else
			{
				try
				{
					process.Kill();
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
		}

		/// <summary>
		/// Starts Tailviewer and waits until it has fully started.
		/// Tailviewer will NOT be closed by the end of this method.
		/// </summary>
		/// <param name="installationPath"></param>
		private static Process Start(string installationPath)
		{
			var path = Path.Combine(installationPath, "Tailviewer.exe");
			var process = Process.Start(path);
			const string reason = "because the application shouldn't exit within the first five seconds";
			process.WaitForExit(5000);

			if (process.HasExited)
			{
				PrintLogFile();
				process.HasExited.Should().BeFalse(reason);
			}

			return process;
		}

		private static void PrintLogFile()
		{
			var fname = Path.Combine(Constants.AppDataLocalFolder, "Tailviewer.log");
			Console.WriteLine(File.ReadAllText(fname));
		}
	}
}