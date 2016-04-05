using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Installer
{
	internal sealed class Installer : IDisposable
	{
		private readonly string _installationPath;

		public Installer(string installationPath)
		{
			_installationPath = installationPath;
		}

		public void Dispose()
		{
		}

		public void Run()
		{
			var start = DateTime.Now;

			try
			{

				RemovePreviousInstallation();
				EnsureInstallationPath();
				InstallNewFiles();
			}
			finally
			{
				var end = DateTime.Now;
				var elapsed = end - start;
				var remaining = TimeSpan.FromSeconds(1) - elapsed;
				if (remaining > TimeSpan.Zero)
				{
					Thread.Sleep(remaining);
				}
			}
		}

		private void RemovePreviousInstallation()
		{
			Delete(_installationPath);
		}

		private void Delete(string path)
		{
			var directory = new DirectoryInfo(path);

			foreach (FileInfo file in directory.GetFiles())
			{
				file.Delete();
			}
			foreach (DirectoryInfo dir in directory.GetDirectories())
			{
				Delete(dir.FullName);
			}
		}

		private void EnsureInstallationPath()
		{
			Directory.CreateDirectory(_installationPath);
		}

		private void InstallNewFiles()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			string prefix = string.Format("{0}.InstallationFiles.", assembly.GetName().Name);
			var allFiles = assembly.GetManifestResourceNames();
			var files = allFiles.Where(x => x.Contains(prefix)).ToList();
			double perFile = 1.0 / files.Count;

			foreach (var file in files)
			{
				var fileName = file.Substring(prefix.Length);
				string destFilePath = Path.Combine(_installationPath, fileName);

				using (var dest = new FileStream(destFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
				using (var source = assembly.GetManifestResourceStream(file))
				{
					const int size = 4096;
					var buffer = new byte[size];

					int read;
					while ((read = source.Read(buffer, 0, size)) > 0)
					{
						dest.Write(buffer, 0, read);
					}
				}
			}
		}
	}
}