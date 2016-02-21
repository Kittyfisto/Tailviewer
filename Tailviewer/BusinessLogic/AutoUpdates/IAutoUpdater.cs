using System;

namespace Tailviewer.BusinessLogic.AutoUpdates
{
	public interface IAutoUpdater
	{
		event Action<VersionInfo> LatestVersionChanged;

		/// <summary>
		/// The version of this application.
		/// </summary>
		Version AppVersion { get; }
		VersionInfo LatestVersion { get; }
	}
}