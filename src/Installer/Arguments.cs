using System;
using System.Reflection;
using log4net;

namespace Installer
{
	public sealed class Arguments
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public readonly Mode Mode;
		public readonly string InstallationPath;

		private Arguments(Mode mode, string installationPath)
		{
			Mode = mode;
			InstallationPath = !string.IsNullOrEmpty(installationPath) ? installationPath : Constants.DefaultApplicationFolder;
		}

		public static Arguments Install()
		{
			return new Arguments(Mode.Install, null);
		}

		public static Arguments Parse(string[] args)
		{
			if (args == null || args.Length < 1)
				return Install();

			var mode = args[0];
			var installationPath = args.Length > 1 ? args[1] : null;
			switch (mode)
			{
				case "update":
					return new Arguments(Mode.Update, installationPath);

				case "silentinstall":
					return new Arguments(Mode.SilentInstall, installationPath);

				case "uninstall":
					return new Arguments(Mode.Uninstall, installationPath);

				default:
					throw new Exception($"Unable to parse '{mode}'");
			}
		}
	}
}