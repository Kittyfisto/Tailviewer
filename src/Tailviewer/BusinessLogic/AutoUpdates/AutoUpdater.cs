using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Authentication;
using System.Threading.Tasks;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.Settings;
using log4net;

namespace Tailviewer.BusinessLogic.AutoUpdates
{
	public sealed class AutoUpdater
		: IAutoUpdater
		, IDisposable
	{
		private const string LatestReleaseUri = "https://api.github.com/repos/brofea/Tailviewer-NEXT/releases/latest";
		private const string UserAgent = "Tailviewer-NEXT";

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
				if (latestVersion != null)
				{
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
				// There's no need to log that the task didn't return any result:
				// we already logged this problem in the task...
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

		public static Uri BuildVersionCheckUri()
		{
			return new Uri(LatestReleaseUri);
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

		internal byte[] Download(Version version, Uri uri)
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

		internal VersionInfo QueryNewestVersions()
		{
			try
			{
				using (var client = new WebClient())
				{
					client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
					client.UseDefaultCredentials = true;
					client.Proxy = _settings.GetWebProxy();
					client.Headers[HttpRequestHeader.UserAgent] = UserAgent;

					Uri uri = BuildVersionCheckUri();

					// For some reason, DownloadData does not accept the certificate offered by some
					// github.io hosts anymore. Forcing the SPM to accept TLS 1.2 on top of its defaults
					// fixes that, but sucks because older client's won't update automatically...
					ServicePointManager.SecurityProtocol |= (SecurityProtocolType)SslProtocols.Tls12;

					Log.InfoFormat("Looking for newest version on '{0}'", uri);
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

		public static void Parse(byte[] data, out VersionInfo latestVersions)
		{
			latestVersions = new VersionInfo(null, null, null, null);

			using (var stream = new MemoryStream(data))
			{
				var serializer = new DataContractJsonSerializer(typeof(GitHubRelease));
				var release = (GitHubRelease) serializer.ReadObject(stream);

				Version stable = ParseVersion(release.TagName);
				Uri stableAddress = FindInstallerAsset(release.Assets);
				latestVersions = new VersionInfo(null, null, stable, stableAddress);
			}
		}

		private static Version ParseVersion(string tagName)
		{
			if (string.IsNullOrEmpty(tagName))
				return null;

			string withoutPrefix = tagName.StartsWith("v", StringComparison.InvariantCultureIgnoreCase)
				? tagName.Substring(1)
				: tagName;

			Version version;
			if (!Version.TryParse(withoutPrefix, out version))
				return null;

			return version;
		}

		private static Uri FindInstallerAsset(GitHubAsset[] assets)
		{
			if (assets == null)
				return null;

			GitHubAsset setup = assets.FirstOrDefault(x =>
				x.Name != null &&
				x.Name.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase) &&
				x.Name.IndexOf("setup", StringComparison.InvariantCultureIgnoreCase) >= 0);

			GitHubAsset installer = setup ?? assets.FirstOrDefault(x =>
				x.Name != null &&
				x.Name.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase));

			if (installer == null)
				return null;

			Uri address;
			return Uri.TryCreate(installer.BrowserDownloadUrl, UriKind.Absolute, out address) ? address : null;
		}

		[DataContract]
		public sealed class GitHubRelease
		{
			[DataMember(Name = "tag_name")]
			public string TagName { get; set; }

			[DataMember(Name = "prerelease")]
			public bool Prerelease { get; set; }

			[DataMember(Name = "assets")]
			public GitHubAsset[] Assets { get; set; }
		}

		[DataContract]
		public sealed class GitHubAsset
		{
			[DataMember(Name = "name")]
			public string Name { get; set; }

			[DataMember(Name = "browser_download_url")]
			public string BrowserDownloadUrl { get; set; }
		}
	}
}
