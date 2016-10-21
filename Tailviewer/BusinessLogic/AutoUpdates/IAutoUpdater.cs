using System;
using System.Threading.Tasks;

namespace Tailviewer.BusinessLogic.AutoUpdates
{
	public interface IAutoUpdater
	{
		/// <summary>
		///     The version of this application.
		/// </summary>
		Version AppVersion { get; }

		/// <summary>
		/// </summary>
		VersionInfo LatestVersion { get; }

		/// <summary>
		/// </summary>
		/// <param name="addNotificationWhenUpToDate">Whether or not a notification should be added when the installed version is up-to-date</param>
		void CheckForUpdates(bool addNotificationWhenUpToDate);

		 /// <summary>
		///     Downloads and installs the latest version.
		/// </summary>
		/// <param name="latest"></param>
		Task Install(VersionInfo latest);

		event Action<VersionInfo> LatestVersionChanged;
	}
}