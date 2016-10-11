using System;

namespace Tailviewer.BusinessLogic.AutoUpdates
{
	public interface IAutoUpdater
	{
		/// <summary>
		///     The version of this application.
		/// </summary>
		Version AppVersion { get; }

		/// <summary>
		/// 
		/// </summary>
		VersionInfo LatestVersion { get; }

		/// <summary>
		/// 
		/// </summary>
		void CheckForUpdatesAsync();

		event Action<VersionInfo> LatestVersionChanged;
	}
}