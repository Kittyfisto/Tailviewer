using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Tailviewer.AcceptanceTests
{
	public abstract class SystemtestBase
	{
		private string _installerPath;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			const string installerName = "Tailviewer-setup.exe";
			var sourcePath = Path.Combine(TestContext.CurrentContext.TestDirectory, installerName);
			var destPath = Path.Combine(Path.GetTempPath(), installerName);
			if (File.Exists(destPath))
				File.Delete(destPath);
			File.Copy(sourcePath, destPath);

			_installerPath = destPath;
		}

		public string InstallerPath => _installerPath;

		protected void Clear(string path)
		{
			if (Directory.Exists(path))
			{
				var di = new DirectoryInfo(path);
				foreach (var file in di.GetFiles())
					file.Delete();
				foreach (var subDir in di.GetDirectories())
					subDir.Delete(true);
			}
			else
			{
				Directory.CreateDirectory(path);
			}
		}

		protected void InstallInto(string installationPath)
		{
			Console.WriteLine("Installing into '{0}'...", installationPath);

			var arguments = new StringBuilder();
			arguments.Append("silentinstall ");
			arguments.AppendFormat("\"{0}\"", installationPath);
			var startInfo = new ProcessStartInfo(_installerPath, arguments.ToString())
			{
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			var process = Process.Start(startInfo);

			var output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();

			if (process.ExitCode != 0)
			{
				TestContext.Progress.WriteLine(output);
				Assert.Fail("Installation failed with exit code {0}", process.ExitCode);
			}
		}
	}
}