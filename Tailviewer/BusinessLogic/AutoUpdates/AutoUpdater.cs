using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Tailviewer.Settings;
using log4net;

namespace Tailviewer.BusinessLogic.AutoUpdates
{
	internal sealed class AutoUpdater
		: IDisposable
	{
		// TODO: Change to production address...
		private const string Server = "http://localhost";
		private const ushort Port = 8080;

		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly Version _currentAppVersion;

		private AutoUpdateSettings _settings;
		private Task _updateTask;

		static AutoUpdater()
		{
			_currentAppVersion = Assembly.GetEntryAssembly().GetName().Version;
		}

		public AutoUpdater(AutoUpdateSettings settings)
		{
			_settings = settings;
			_updateTask = Task.Factory.StartNew(CheckForUpdate);
		}

		private static Version CurrentAppVersion
		{
			get { return _currentAppVersion; }
		}

		public void Dispose()
		{
		}

		internal static Uri BuildVersionCheckUri(Version currentAppVersion)
		{
			string address = string.Format("{0}:{1}/check_version?major={2}&minor={3}&build={4}",
			                               Server,
			                               Port,
			                               currentAppVersion.Major,
			                               currentAppVersion.Minor,
			                               currentAppVersion.Build);
			var uri = new Uri(address);
			return uri;
		}

		private void CheckForUpdate()
		{
			try
			{
				using (var client = new WebClient())
				{
					client.UseDefaultCredentials = true;
					client.Proxy = WebRequest.GetSystemWebProxy();
					Uri uri = BuildVersionCheckUri(CurrentAppVersion);
					byte[] data = client.DownloadData(uri);
				}
			}
			catch (WebException e)
			{
				Log.WarnFormat("Unable to query the newest version: {0}", e);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while querying newest version: {0}", e);
			}
		}
	}
}