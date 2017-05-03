using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.Settings;
using log4net;

namespace Tailviewer.BusinessLogic.AutoUpdates
{
	internal sealed class AutoUpdater
		: IAutoUpdater
		, IDisposable
	{
		private const string Server = "https://kittyfisto.github.io";
		private const string VersionFile = "Tailviewer/downloads/version.xml";
		private const string DownloadServer = "https://github.com/Kittyfisto/Tailviewer/releases/download/";

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly Version CurrentAppVersion;

		private readonly object _syncRoot;
		private readonly IActionCenter _actionCenter;
		private readonly IAutoUpdateSettings _settings;
		private readonly List<Action<VersionInfo>> _latestVersionChanged;

		private VersionInfo _latestVersion;

		static AutoUpdater()
		{
			try
			{
				CurrentAppVersion = Assembly.GetCallingAssembly().GetName().Version;
			}
			catch (Exception e)
			{
				Log.WarnFormat("Unable to read the current version: {0}", e);
			}
		}

		public AutoUpdater(IActionCenter actionCenter, IAutoUpdateSettings settings)
		{
			if (actionCenter == null)
				throw new ArgumentNullException(nameof(actionCenter));
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));

			_syncRoot = new object();
			_actionCenter = actionCenter;
			_settings = settings;
			_latestVersionChanged = new List<Action<VersionInfo>>();
		}

		public void CheckForUpdates(bool addNotificationWhenUpToDate)
		{
			var updateTask = Task<VersionInfo>.Factory.StartNew(QueryNewestVersions);
			updateTask.ContinueWith(task => OnVersionChecked(task, addNotificationWhenUpToDate));
		}

		public Task Install(VersionInfo latest)
		{
			return Task.Factory.StartNew(() => DownloadAndInstall(latest.Stable, latest.StableAddress));
		}

		public event Action<VersionInfo> LatestVersionChanged
		{
			add
			{
				lock (_syncRoot)
				{
					_latestVersionChanged.Add(value);
					if (_latestVersion != null)
					{
						value(_latestVersion);
					}
				}
			}
			remove
			{
				lock (_syncRoot)
				{
					_latestVersionChanged.Remove(value);
				}
			}
		}

		private void OnVersionChecked(Task<VersionInfo> task, bool addNotificationWhenUpToDate)
		{
			try
			{
				VersionInfo latestVersion = task.Result;
				LatestVersion = latestVersion;

				Version latest = latestVersion.Stable;
				Version current = AppVersion;
				if (current != null && latest != null && latest > current)
				{
					var message = string.Format("A newer version ({0}) is available to be downloaded", latest);
					Log.InfoFormat(message);
					_actionCenter.Add(Notification.CreateInfo("Check for updates", message));
				}
				else
				{
					const string message = "Running the latest version";
					Log.InfoFormat(message);

					if (addNotificationWhenUpToDate)
					{
						_actionCenter.Add(Notification.CreateInfo("Check for updates", message));
					}
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while querying newest version: {0}", e);
			}
		}

		public Version AppVersion => CurrentAppVersion;

		public VersionInfo LatestVersion
		{
			get { return _latestVersion; }
			private set
			{
				lock (_syncRoot)
				{
					if (value == _latestVersion)
						return;

					_latestVersion = value;

					foreach (var fn in _latestVersionChanged)
					{
						fn(value);
					}
				}
			}
		}

		public void Dispose()
		{
		}

		internal static Uri BuildVersionCheckUri()
		{
			string address = string.Format("{0}/{1}", Server, VersionFile);
			var uri = new Uri(address);
			return uri;
		}

		private void DownloadAndInstall(Version version, Uri uri)
		{
			try
			{
				var data = Download(version, uri);
				var fileName = SaveInstaller(version, uri, data);
				StartInstallation(fileName);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Unable to perform update: {0}", e);
			}
		}

		private byte[] Download(Version version, Uri uri)
		{
			try
			{
				Log.InfoFormat("Downloading 'v{0}' from '{1}'", version, uri);

				using (var client = new WebClient())
				{
					client.UseDefaultCredentials = true;
					client.Proxy = _settings.GetWebProxy();

					var data = client.DownloadData(uri);
					return data;
				}
			}
			catch (WebException e)
			{
				Log.WarnFormat("Unable to download the newest version: {0}", e);
				throw;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while downloading newest version: {0}", e);
				throw;
			}
		}

		private string SaveInstaller(Version version, Uri uri, byte[] data)
		{
			var fileName = Path.GetFileName(uri.AbsolutePath);
			var fullPath = Path.Combine(Constants.DownloadFolder, version.ToString(), fileName);

			Log.InfoFormat("Saving installer to '{0}'", fullPath);

			var directory = Path.GetDirectoryName(fullPath);
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			File.WriteAllBytes(fullPath, data);
			return fullPath;
		}

		private void StartInstallation(string fileName)
		{
			try
			{
				var arguments = string.Format("update \"{0}\"", Constants.ApplicationFolder);
				Process.Start(new ProcessStartInfo(fileName, arguments));
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while starting updater: {0}", e);
			}
		}

		private VersionInfo QueryNewestVersions()
		{
			try
			{
				using (var client = new WebClient())
				{
					client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
					client.UseDefaultCredentials = true;
					client.Proxy = _settings.GetWebProxy();

					Uri uri = BuildVersionCheckUri();

					Log.InfoFormat("Looking for newest version on '{0}", uri);
					byte[] data = client.DownloadData(uri);

					Log.DebugFormat("Parsing response ({0} bytes)",
					                data?.Length.ToString(CultureInfo.InvariantCulture) ?? "null");

					VersionInfo versions;
					Parse(data, out versions);
					Log.InfoFormat("Most recent versions: {0}", versions);
					return versions;
				}
			}
			catch (WebException e)
			{
				Log.WarnFormat("Unable to query the newest version: {0}", e);

				var message = e.Message;
				_actionCenter.Add(Notification.CreateError("Check for updates", message));

				return null;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while querying newest version: {0}", e);

				var message = string.Format("There was an unexpected error: {0}", e.Message);
				_actionCenter.Add(Notification.CreateError("Check for updates", message));

				return null;
			}
		}

		internal static void Parse(byte[] data, out VersionInfo latestVersions)
		{
			latestVersions = new VersionInfo(null, null, null, null);

			using (var stream = new MemoryStream(data))
			using (XmlReader reader = XmlReader.Create(stream))
			{
				while (reader.Read())
				{
					switch (reader.Name)
					{
						case "versions":
							ReadVersions(reader.ReadSubtree(), out latestVersions);
							break;
					}
				}
			}
		}

		private static void ReadVersions(XmlReader versions, out VersionInfo latestVersions)
		{
			Version beta = null;
			Uri betaAddress = null;
			Version stable = null;
			Uri stableAddress = null;

			while (versions.Read())
			{
				switch (versions.Name)
				{
					case "stable":
						stable = ReadVersion(versions, out stableAddress);
						break;

					case "beta":
						beta = ReadVersion(versions, out betaAddress);
						break;
				}
			}

			latestVersions = new VersionInfo(beta, betaAddress, stable, stableAddress);
		}

		private static Version ReadVersion(XmlReader versions, out Uri address)
		{
			var path = versions.GetAttribute("path");
			var fullPath = string.Format("{0}{1}", DownloadServer, path);
			if (!Uri.TryCreate(fullPath, UriKind.Absolute, out address))
				return null;

			int major;
			int minor;
			int patch;
			int build;
			if (!int.TryParse(versions.GetAttribute("major"), NumberStyles.Integer, CultureInfo.InvariantCulture, out major))
				return null;
			if (!int.TryParse(versions.GetAttribute("minor"), NumberStyles.Integer, CultureInfo.InvariantCulture, out minor))
				return null;
			if (!int.TryParse(versions.GetAttribute("patch"), NumberStyles.Integer, CultureInfo.InvariantCulture, out patch))
				return null;
			if (!int.TryParse(versions.GetAttribute("build"), NumberStyles.Integer, CultureInfo.InvariantCulture, out build))
				return null;

			return new Version(major, minor, patch, build);
		}
	}
}