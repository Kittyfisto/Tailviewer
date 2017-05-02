using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Installer.Exceptions;
using Metrolib;
using log4net;
using Microsoft.Win32;

namespace Installer
{
	internal sealed class Installer : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Assembly _assembly;
		private readonly List<string> _files;
		private readonly Size _installationSize;
		private readonly string _prefix;
		private Size _installedSize;
		private double _progress;
		private string _installationPath;

		public Installer()
		{
			_assembly = Assembly.GetExecutingAssembly();
			_prefix = "Installer.InstallationFiles.";
			string[] allFiles = _assembly.GetManifestResourceNames();
			_files = allFiles.Where(x => x.Contains(_prefix)).ToList();
			_installationSize = _files.Aggregate(Size.Zero, (size, fileName) => size + Filesize(fileName));
		}

		public Size InstallationSize
		{
			get { return _installationSize; }
		}

		public double Progress
		{
			get { return _progress; }
		}

		public void Dispose()
		{
		}

		private Size Filesize(string fileName)
		{
			using (Stream stream = _assembly.GetManifestResourceStream(fileName))
			{
				if (stream == null)
					return Size.Zero;

				return Size.FromBytes(stream.Length);
			}
		}

		public void Run(string installationPath)
		{
			DateTime start = DateTime.Now;
			_installationPath = installationPath;

			try
			{
				RemovePreviousInstallation(installationPath);
				EnsureInstallationPath(installationPath);
				InstallNewFiles(installationPath);
				WriteRegistry();

				Log.InfoFormat("Installation succeeded");
			}
			catch (Exception e)
			{
				Log.FatalFormat("Unable to complete installation: {0}", e);
				throw;
			}
			finally
			{
				DateTime end = DateTime.Now;
				TimeSpan elapsed = end - start;
				TimeSpan remaining = TimeSpan.FromMilliseconds(500) - elapsed;
				if (remaining > TimeSpan.Zero)
				{
					Thread.Sleep(remaining);
				}
			}
		}

		private void RemovePreviousInstallation(string installationPath)
		{
			foreach (string file in _files)
			{
				string fullFilePath = DestFilePath(installationPath, file);
				DeleteFile(fullFilePath);
			}
		}

		private void DeleteFile(string filePath)
		{
			string name = Path.GetFileName(filePath);
			string dir = Path.GetDirectoryName(filePath);

			Log.InfoFormat("Removing {0}", filePath);

			int tries = 0;
			Exception lastException = null;
			while(++tries < 10)
			{
				try
				{
					File.Delete(filePath);
					break;
				}
				catch (DirectoryNotFoundException)
				{
					// This is great, one thing less to delete...
					break;
				}
				catch (Exception e)
				{
					Thread.Sleep(TimeSpan.FromMilliseconds(100));
					lastException = e;
				}
			}

			if (lastException != null)
				throw new DeleteFileException(name, dir, lastException);
		}

		private void EnsureInstallationPath(string installationPath)
		{
			Directory.CreateDirectory(installationPath);
		}

		private void InstallNewFiles(string installationPath)
		{
			foreach (string file in _files)
			{
				string destFilePath = DestFilePath(installationPath, file);
				CopyFile(destFilePath, file);
			}
		}

		private void WriteRegistry()
		{
			var uninstallPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
			var uninstall = Registry.LocalMachine.OpenSubKey(uninstallPath, true);
			if (uninstall == null)
			{
				Log.ErrorFormat("Unable to locate '{0}', this shouldn't really happen...", uninstallPath);
				return;
			}

			var iconPath = Path.Combine(_installationPath, "Icons", "Tailviewer.ico");

			var program = uninstall.CreateSubKey(Constants.ApplicationTitle);
			program.SetValue("DisplayName", Constants.ApplicationTitle, RegistryValueKind.String);
			program.SetValue("DisplayIcon", iconPath, RegistryValueKind.String);
			program.SetValue("UninstallString", "TODO", RegistryValueKind.String);
			program.SetValue("DisplayVersion", Constants.ApplicationVersion, RegistryValueKind.String);
			program.SetValue("Publisher", Constants.Publisher, RegistryValueKind.String);
			program.SetValue("EstimatedSize", _installationSize.Kilobytes, RegistryValueKind.DWord);
		}

		private string DestFilePath(string installationPath, string file)
		{
			string fileName = file.Substring(_prefix.Length);
			var patchedFileName = Patch(fileName);
			string destFilePath = Path.Combine(installationPath, patchedFileName);
			return destFilePath;
		}

		private string Patch(string fileName)
		{
			return fileName.Replace("Fonts.", "Fonts\\")
				.Replace("Icons.", "Icons\\")
				.Replace("x64.", "x64\\")
				.Replace("x86.", "x86\\");
		}

		private void CopyFile(string destFilePath, string sourceFilePath)
		{
			string directory = Path.GetDirectoryName(destFilePath);
			string fileName = Path.GetFileName(destFilePath);

			try
			{
				CreateDirectory(directory);

				Log.InfoFormat("Writing file '{0}'", destFilePath);

				using (var dest = new FileStream(destFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
				using (Stream source = _assembly.GetManifestResourceStream(sourceFilePath))
				{
					const int size = 4096;
					var buffer = new byte[size];

					int read;
					while ((read = source.Read(buffer, 0, size)) > 0)
					{
						dest.Write(buffer, 0, read);
						_installedSize += Size.FromBytes(read);
						_progress = _installedSize/_installationSize;
					}
				}
			}
			catch (Exception e)
			{
				throw new CopyFileException(fileName, directory, e);
			}
		}

		private void CreateDirectory(string directory)
		{
			if (!Directory.Exists(directory))
			{
				Log.InfoFormat("Creating directory '{0}'", directory);
				Directory.CreateDirectory(directory);
			}
		}

		public void Launch()
		{
			string app = Path.Combine(_installationPath, "Tailviewer.exe");
			Process.Start(app);
		}
	}
}