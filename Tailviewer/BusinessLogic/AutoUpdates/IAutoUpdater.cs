using System;

namespace Tailviewer.BusinessLogic.AutoUpdates
{
	public interface IAutoUpdater
	{
		/// <summary>
		///     The version of this application.
		/// </summary>
		Version AppVersion { get; }

		VersionInfo LatestVersion { get; }
		event Action<VersionInfo> LatestVersionChanged;
	}
}