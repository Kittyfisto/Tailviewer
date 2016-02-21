using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Tailviewer.Settings;
using log4net;

namespace Tailviewer.BusinessLogic.AutoUpdates
{
	internal sealed class AutoUpdater
		: IDisposable, IAutoUpdater
	{
#if DEBUG
		private const string Server = "http://localhost";
		private const ushort Port = 8080;
#else
		private const string Server = "http://tailviewer-1223.appspot.com";
		private const ushort Port = 80;
#endif

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly Version CurrentAppVersion;

		private readonly Task<VersionInfo> _updateTask;
		private readonly object _syncRoot;
		private AutoUpdateSettings _settings;
		private VersionInfo _latestVersion;
		private readonly List<Action<VersionInfo>> _latestVersionChanged;

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

		public AutoUpdater(AutoUpdateSettings settings)
		{
			_syncRoot = new object();
			_settings = settings;
			_latestVersionChanged = new List<Action<VersionInfo>>();
			_updateTask = Task<VersionInfo>.Factory.StartNew(QueryNewestVersions);
			_updateTask.ContinueWith(OnVersionChecked);
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
				_latestVersionChanged.Remove(value);
			}
		}

		private void OnVersionChecked(Task<VersionInfo> task)
		{
			try
			{
				VersionInfo latest = task.Result;
				LatestVersion = latest;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while querying newest version: {0}", e);
			}
		}

		public Version AppVersion
		{
			get { return CurrentAppVersion; }
		}

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
			string address = string.Format("{0}:{1}/query_version",
			                               Server,
			                               Port);
			var uri = new Uri(address);
			return uri;
		}

		private VersionInfo QueryNewestVersions()
		{
			try
			{
				using (var client = new WebClient())
				{
					client.UseDefaultCredentials = true;
					client.Proxy = WebRequest.GetSystemWebProxy();
					Uri uri = BuildVersionCheckUri();

					Log.DebugFormat("Looking for newest version on '{0}", uri);
					byte[] data = client.DownloadData(uri);

					Log.DebugFormat("Parsing response ({0} bytes)",
					                data != null ? data.Length.ToString(CultureInfo.InvariantCulture) : "null");

					VersionInfo versions;
					Parse(data, out versions);
					Log.InfoFormat("Most recent versions: {0}", versions);
					return versions;
				}
			}
			catch (WebException e)
			{
				Log.WarnFormat("Unable to query the newest version: {0}", e);
				return null;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while querying newest version: {0}", e);
				return null;
			}
		}

		internal static void Parse(byte[] data, out VersionInfo latestVersions)
		{
			latestVersions = new VersionInfo(null, null);

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
			Version latestBeta = null;
			Version latestRelease = null;

			while (versions.Read())
			{
				switch (versions.Name)
				{
					case "release":
						latestRelease = ReadVersion(versions);
						break;

					case "beta":
						latestBeta = ReadVersion(versions);
						break;
				}
			}

			latestVersions = new VersionInfo(latestBeta, latestRelease);
		}

		private static Version ReadVersion(XmlReader versions)
		{
			int major = int.Parse(versions.GetAttribute("major"));
			int minor = int.Parse(versions.GetAttribute("minor"));
			int build = int.Parse(versions.GetAttribute("build"));
			return new Version(major, minor, build);
		}
	}
}