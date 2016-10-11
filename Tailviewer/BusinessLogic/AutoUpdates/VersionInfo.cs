using System;

namespace Tailviewer.BusinessLogic.AutoUpdates
{
	public sealed class VersionInfo
	{
		public readonly Version Beta;
		public readonly Uri BetaAddress;
		public readonly Version Stable;
		public readonly Uri StableAddress;

		public VersionInfo(Version beta,
		                   Uri betaAddress,
		                   Version stable,
		                   Uri stableAddress)
		{
			Beta = beta;
			BetaAddress = betaAddress;
			Stable = stable;
			StableAddress = stableAddress;
		}

		public override string ToString()
		{
			return string.Format("Beta: {0}, Stable: {1}",
			                     Beta != null ? Beta.ToString() : "none",
			                     Stable != null ? Stable.ToString() : "none");
		}
	}
}