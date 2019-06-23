using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Installer.Applications.Install;
using Installer.Exceptions;
using IWshRuntimeLibrary;
using log4net;
using Metrolib;
using Microsoft.Win32;
using File = System.IO.File;

namespace Installer
{
	internal sealed class Installer : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Assembly _assembly;
		private readonly List<string> _files;
		private readonly string _prefix;
		private string _installationPath;
		private Size _installedSize;

		public Installer()
		{
			_assembly = Assembly.GetExecutingAssembly();
			_prefix = "InstallationFiles\\";
			var allFiles = _assembly.GetManifestResourceNames();
			_files = allFiles.Where(x => x.Contains(_prefix)).ToList();
			InstallationSize = _files.Aggregate(Size.Zero, (size, fileName) => size + Filesize(fileName));
		}

		public Size InstallationSize { get; }

		public double Progress { get; private set; }

		private string IconPath => Path.Combine(_installationPath, "Icons", "Tailviewer.ico");

		public void Dispose()
		{
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

		public void Run(string installationPath)
		{
			var start = DateTime.Now;
			_installationPath = installationPath;

			try
			{
				ExitTailviewer(installationPath);
				RemovePreviousInstallation(installationPath);
				EnsureInstallationPath(installationPath);
				InstallNewFiles(installationPath);
				WriteRegistry();
				CreateStartMenuEntry();

				Log.InfoFormat("Installation succeeded");
			}
			catch (Exception e)
			{
				Log.FatalFormat("Unable to complete installation: {0}", e);
				throw;
			}
			finally
			{
				var end = DateTime.Now;
				var elapsed = end - start;
				var remaining = TimeSpan.FromMilliseconds(500) - elapsed;
				if (remaining > TimeSpan.Zero)
					Thread.Sleep(remaining);
			}
		}

		private void ExitTailviewer(string installationPath)
		{
			try
			{
				// We'll first try to be nice and ask Tailviewer to exit...
				var executableFilePath = Path.Combine(installationPath, Constants.ApplicationExecutable);
				var process = TryGetProcess(executableFilePath);
				if (process != null)
				{
					Log.InfoFormat("Found running tailviewer process, closing it...");

					using (process)
					{
						if (!process.CloseMainWindow())
						{
							Log.WarnFormat("Process (PID {0}) refuses to be closed, killing it...", process.Id);
						}

						process.Kill();

						// Yes, we might have killed the process, for but reasons unknown,
						// it takes some time until the operating system allows us actually
						// access the files in question, so we'll just have to busy wait for
						// a while...
						foreach (var file in _files)
						{
							var destFilePath = DestFilePath(installationPath, file);
							WaitUntilFileCanBeAccessed(destFilePath);
						}
					}
				}
				else
				{
					Log.InfoFormat("Did not find running tailviewer process");
				}
			}
			catch (Exception e)
			{
				Log.WarnFormat("Caught exception while trying to find running Tailviewer process, assuming there is none...\r\n{0}", e);
			}
		}

		private static Process TryGetProcess(string executableFilePath)
		{
			var fileName = Path.GetFileNameWithoutExtension(executableFilePath);
			var processes = Process.GetProcessesByName(fileName);
			foreach (var process in processes)
			{
				var fullFilePath = process?.MainModule?.FileName;
				if (fullFilePath == executableFilePath)
				{
					return process;
				}
			}

			return null;
		}

		private static void WaitUntilFileCanBeAccessed(string filePath)
		{
			var stopwatch = Stopwatch.StartNew();
			var timeout = TimeSpan.FromSeconds(5);
			while (stopwatch.Elapsed < timeout
			       && !CanAccessFile(filePath))
			{
				Thread.Sleep(TimeSpan.FromMilliseconds(100));
			}

			if (!CanAccessFile(filePath))
			{
				Log.ErrorFormat("Still unable to open '{0}' for writing, even after waiting for {1}s!",
				                filePath, (int) timeout.TotalSeconds);
			}
			else
			{
				Log.InfoFormat("{0} can be accessed", filePath);
			}
		}

		private static bool CanAccessFile(string file)
		{
			try
			{
				using (File.OpenWrite(file))
				{}

				return true;
			}
			catch (DirectoryNotFoundException)
			{
				return true;
			}
			catch (FileNotFoundException)
			{
				return true;
			}
			catch (IOException e)
			{
				// We expect this exception type to be thrown in case the file cannot be accessed because it's
				// still in use...
				Log.DebugFormat("Unable to access '{0}': {1}", file, e);
				return false;
			}
			catch (Exception e)
			{
				Log.WarnFormat("Unable to access '{0}' for an unexpected reason: {1}", file, e);
				return false;
			}
		}

		private void RemovePreviousInstallation(string installationPath)
		{
			if (Directory.Exists(installationPath))
			{
				var existingFiles = Directory.EnumerateFiles(installationPath).ToList();
				foreach (var file in existingFiles)
				{
					DeleteFile(file);
				}
			}
		}

		private void DeleteFile(string filePath)
		{
			var name = Path.GetFileName(filePath);
			var dir = Path.GetDirectoryName(filePath);

			Log.InfoFormat("Removing {0}", filePath);

			var tries = 0;
			Exception lastException = null;
			while (++tries < 10)
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

			if (lastException != null)
				throw new DeleteFileException(name, dir, lastException);
		}

		private void EnsureInstallationPath(string installationPath)
		{
			Directory.CreateDirectory(installationPath);
		}

		private void InstallNewFiles(string installationPath)
		{
			foreach (var file in _files)
			{
				var destFilePath = DestFilePath(installationPath, file);
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

			var program = uninstall.CreateSubKey(Constants.ApplicationTitle);
			program.SetValue("DisplayName", Constants.ApplicationTitle, RegistryValueKind.String);
			program.SetValue("DisplayIcon", IconPath, RegistryValueKind.String);
			program.SetValue("UninstallString", "TODO", RegistryValueKind.String);
			program.SetValue("DisplayVersion", Constants.ApplicationVersion, RegistryValueKind.String);
			program.SetValue("Publisher", Constants.Publisher, RegistryValueKind.String);
			program.SetValue("EstimatedSize", InstallationSize.Kilobytes, RegistryValueKind.DWord);
		}

		private void CreateStartMenuEntry()
		{
			var startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms);
			var shortcutFolder = Path.Combine(startMenuPath, Constants.ApplicationTitle);
			if (!Directory.Exists(shortcutFolder))
				Directory.CreateDirectory(shortcutFolder);
			var shell = new WshShell();
			var tailviewerLink = Path.Combine(shortcutFolder, "Tailviewer.lnk");
			var shortcut = (IWshShortcut) shell.CreateShortcut(tailviewerLink);
			shortcut.TargetPath = Path.Combine(_installationPath, "Tailviewer.exe");
			shortcut.IconLocation = IconPath;
			shortcut.Arguments = "";
			shortcut.Description = "Open & Free log file viewer";
			shortcut.Save();
		}

		private string DestFilePath(string installationPath, string file)
		{
			var fileName = file.Substring(_prefix.Length);
			var destFilePath = Path.Combine(installationPath, fileName);
			return destFilePath;
		}

		private void CopyFile(string destFilePath, string sourceFilePath)
		{
			var directory = Path.GetDirectoryName(destFilePath);
			var fileName = Path.GetFileName(destFilePath);

			try
			{
				CreateDirectory(directory);

				Log.InfoFormat("Writing file '{0}'", destFilePath);

				using (var dest = new FileStream(destFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
				using (var source = _assembly.GetManifestResourceStream(sourceFilePath))
				{
					const int size = 4096;
					var buffer = new byte[size];

					dest.SetLength(0); //< We might write less bytes than the file previously had!

					int read;
					while ((read = source.Read(buffer, 0, size)) > 0)
					{
						dest.Write(buffer, 0, read);
						_installedSize += Size.FromBytes(read);
						Progress = _installedSize / InstallationSize;
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
			var app = Path.Combine(_installationPath, "Tailviewer.exe");
			Launcher.RunAsDesktopUser(app);
		}
	}
}