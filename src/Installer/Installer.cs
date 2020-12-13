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
		private readonly List<string> _extractedFiles;
		private readonly string _prefix;
		private string _installationPath;
		private Size _installedSize;

		public Installer()
		{
			_assembly = Assembly.GetExecutingAssembly();
			_prefix = "InstallationFiles\\";
			var allFiles = _assembly.GetManifestResourceNames();
			_extractedFiles = allFiles.Where(x => x.Contains(_prefix)).ToList();
			InstallationSize = _extractedFiles.Aggregate(Size.Zero, (size, fileName) => size + Filesize(fileName));
			InstallationSize += Filesize(ThisInstallerPath);
		}

		public IReadOnlyList<string> InstallationFileNames
		{
			get { return _extractedFiles.Select(x => x.Replace(_prefix, "")).ToList(); }
		}

		public Size InstallationSize { get; }

		public double Progress { get; private set; }

		private string IconPath => Path.Combine(_installationPath, "Icons", "Tailviewer.ico");

		public void Dispose()
		{
		}

		private Size Filesize(string fileName)
		{
			if (Path.IsPathRooted(fileName))
			{
				return Size.FromBytes(new FileInfo(fileName).Length);
			}

			using (var stream = _assembly.GetManifestResourceStream(fileName))
			{
				if (stream == null)
					return Size.Zero;

				return Size.FromBytes(stream.Length);
			}
		}

		public void Install(string installationPath)
		{
			var start = DateTime.Now;
			_installationPath = installationPath;

			try
			{
				ExitTailviewer(installationPath);
				RemovePreviousInstallation(installationPath);
				EnsureInstallationPath(installationPath);
				InstallNewFiles(installationPath);
				CopySetup(installationPath);
				WriteRegistry(installationPath);
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

		public void Uninstall(string installationPath)
		{
			try
			{
				KillTailviewer(installationPath);

				var subFolders = new HashSet<string>();
				foreach (var installedFile in InstallationFileNames)
				{
					var fullInstalledPath = Path.Combine(installationPath, installedFile);
					TryDeleteFile(fullInstalledPath);

					var folder = Path.GetDirectoryName(fullInstalledPath);
					subFolders.Add(folder);
				}

				foreach (var subFolder in subFolders)
				{
					if (subFolder != installationPath)
						TryDeleteDirectory(subFolder);
				}

				TryDeleteDirectory(installationPath);
				DeleteRegistry();
				DeleteStartMenuEntry();
			}
			catch (Exception e)
			{
				Log.FatalFormat("Unable to complete uninstallation: {0}", e);
				throw;
			}
		}

		private static string ThisInstallerPath
		{
			get
			{
				UriBuilder uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
				string sourceSetupPath = Uri.UnescapeDataString(uri.Path);
				return sourceSetupPath;
			}
		}

		private void CopySetup(string installationPath)
		{
			string sourceSetupPath = ThisInstallerPath;
			var destSetupPath = Path.Combine(installationPath, "Tailviewer-setup.exe");

			File.Copy(sourceSetupPath, destSetupPath);
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
						foreach (var file in _extractedFiles)
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

			Log.DebugFormat("Removing {0}", filePath);

			var tries = 0;
			Exception lastException = null;
			while (++tries < 10)
				try
				{
					File.Delete(filePath);
					Log.InfoFormat("Removed {0}", filePath);
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
			foreach (var file in _extractedFiles)
			{
				var destFilePath = DestFilePath(installationPath, file);
				CopyFile(destFilePath, file);
			}
		}
		
		const string UninstallRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";

		private void WriteRegistry(string installationPath)
		{
			var uninstall = Registry.LocalMachine.OpenSubKey(UninstallRegistryPath, true);
			if (uninstall == null)
			{
				Log.ErrorFormat("Unable to locate '{0}', this shouldn't really happen...", UninstallRegistryPath);
				return;
			}

			var program = uninstall.CreateSubKey(Constants.ApplicationTitle);
			program.SetValue("DisplayName", Constants.ApplicationTitle, RegistryValueKind.String);
			program.SetValue("DisplayIcon", IconPath, RegistryValueKind.String);
			program.SetValue("UninstallString", string.Format("CMD.exe /C \"{0}\"", Path.Combine(installationPath, "Uninstall.cmd")), RegistryValueKind.String);
			program.SetValue("DisplayVersion", Constants.ApplicationVersion, RegistryValueKind.String);
			program.SetValue("Publisher", Constants.Publisher, RegistryValueKind.String);
			program.SetValue("EstimatedSize", InstallationSize.Kilobytes, RegistryValueKind.DWord);
		}

		private void DeleteRegistry()
		{
			var uninstall = Registry.LocalMachine.OpenSubKey(UninstallRegistryPath, true);
			if (uninstall == null)
			{
				Log.ErrorFormat("Unable to locate '{0}', this shouldn't really happen...", UninstallRegistryPath);
				return;
			}

			uninstall.DeleteSubKeyTree(Constants.ApplicationTitle);
		}

		static readonly string StartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms);
		static readonly string ShortcutFolder = Path.Combine(StartMenuPath, Constants.ApplicationTitle);
		static readonly string TailviewerLink = Path.Combine(ShortcutFolder, "Tailviewer.lnk");

		private void CreateStartMenuEntry()
		{
			if (!Directory.Exists(ShortcutFolder))
				Directory.CreateDirectory(ShortcutFolder);
			var shell = new WshShell();
			var shortcut = (IWshShortcut) shell.CreateShortcut(TailviewerLink);
			shortcut.TargetPath = Path.Combine(_installationPath, "Tailviewer.exe");
			shortcut.IconLocation = IconPath;
			shortcut.Arguments = "";
			shortcut.Description = "Open & Free log file viewer";
			shortcut.Save();
		}

		private void DeleteStartMenuEntry()
		{
			if (File.Exists(TailviewerLink))
				File.Delete(TailviewerLink);
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

		private void KillTailviewer(string installationPath)
		{
			var normalizedInstallationPath = NormalizePath(installationPath);

			var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Constants.ApplicationExecutable));
			foreach (var process in processes)
			{
				var folder = NormalizePath(Path.GetDirectoryName(process?.MainModule?.FileName));
				if (Equals(folder, normalizedInstallationPath))
				{
					Kill(process);
				}
			}
		}

		private void Kill(Process process)
		{
			Log.InfoFormat("Killing running tailviewer (PID: {0})...", process.Id);

			process.Kill();
		}

		private static string NormalizePath(string path)
		{
			return Path.GetFullPath(new Uri(path).LocalPath)
			           .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
			           .ToUpperInvariant();
		}

		private void CreateDirectory(string directory)
		{
			if (!Directory.Exists(directory))
			{
				Log.InfoFormat("Creating directory '{0}'", directory);
				Directory.CreateDirectory(directory);
			}
		}

		private static void TryDeleteDirectory(string fullInstalledPath)
		{
			try
			{
				Log.DebugFormat("Deleting '{0}'...", fullInstalledPath);
				Directory.Delete(fullInstalledPath);
				Log.DebugFormat("Deleted '{0}'", fullInstalledPath);
			}
			catch (Exception e)
			{
				Log.WarnFormat("Unable to delete directory: {0}", e.Message);
			}
		}

		private void TryDeleteFile(string fullInstalledPath)
		{
			try
			{
				DeleteFile(fullInstalledPath);
			}
			catch (Exception e)
			{
				Log.WarnFormat("Unable to delete file: {0}", e.Message);
			}
		}

		public void Launch()
		{
			var app = Path.Combine(_installationPath, "Tailviewer.exe");
			Launcher.RunAsDesktopUser(app);
		}
	}
}