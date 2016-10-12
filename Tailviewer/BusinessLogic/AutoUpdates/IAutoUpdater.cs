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
		void CheckForUpdates();

		 /// <summary>
		///     Downloads and installs the latest version.
		/// </summary>
		/// <param name="latest"></param>
		Task Install(VersionInfo latest);

		event Action<VersionInfo> LatestVersionChanged;
	}
}