using System;

namespace Tailviewer.BusinessLogic.AutoUpdates
{
	internal sealed class VersionInfo
	{
		public readonly Version Beta;
		public readonly Version Release;

		public VersionInfo(Version beta, Version release)
		{
			Beta = beta;
			Release = release;
		}

		public override string ToString()
		{
			return string.Format("Beta: {0}, Release: {1}",
			                     Beta != null ? Beta.ToString() : "none",
			                     Release != null ? Release.ToString() : "none");
		}
	}
}