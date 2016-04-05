using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Metrolib;

namespace Installer
{
	internal sealed class Installer : IDisposable
	{
		private readonly List<string> _files;
		private readonly Assembly _assembly;
		private readonly string _prefix;
		private readonly Size _installationSize;

		public Size InstallationSize
		{
			get { return _installationSize; }
		}

		public Installer()
		{
			_assembly = Assembly.GetExecutingAssembly();
			_prefix = string.Format("{0}.InstallationFiles.", _assembly.GetName().Name);
			var allFiles = _assembly.GetManifestResourceNames();
			_files = allFiles.Where(x => x.Contains(_prefix)).ToList();
			_installationSize = _files.Aggregate(Size.Zero, (size, fileName) => size + Filesize(fileName));
		}

		private Size Filesize(string fileName)
		{
			using (var stream = _assembly.GetManifestResourceStream(fileName))
			{
				if (stream == null)
					return Size.Zero;

				return Size.FromBytes(stream.Length);
			}
		}

		public void Dispose()
		{
		}

		public void Run(string installationPath)
		{
			var start = DateTime.Now;

			try
			{
				RemovePreviousInstallation(installationPath);
				EnsureInstallationPath(installationPath);
				InstallNewFiles(installationPath);
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

		private void RemovePreviousInstallation(string installationPath)
		{
			Delete(installationPath);
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

		private void EnsureInstallationPath(string installationPath)
		{
			Directory.CreateDirectory(installationPath);
		}

		private void InstallNewFiles(string installationPath)
		{
			double perFile = 1.0 / _files.Count;

			foreach (var file in _files)
			{
				var fileName = file.Substring(_prefix.Length);
				string destFilePath = Path.Combine(installationPath, fileName);

				using (var dest = new FileStream(destFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
				using (var source = _assembly.GetManifestResourceStream(file))
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