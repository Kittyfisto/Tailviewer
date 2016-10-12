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
			InstallationPath = installationPath;
		}

		public static Arguments Install()
		{
			return new Arguments(Mode.Install, null);
		}

		public static Arguments Update(string installationPath)
		{
			return new Arguments(Mode.Update, installationPath);
		}

		public static Arguments SilentInstall(string installationPath)
		{
			return new Arguments(Mode.SilentInstall, installationPath);
		}

		public static Arguments Parse(string[] args)
		{
			if (args == null || args.Length < 2)
				return Install();

			var mode = args[0];
			switch (mode)
			{
				case "update":
					return Update(args[1]);

				case "silentinstall":
					return SilentInstall(args[1]);

				default:
					Log.WarnFormat("Unable to parse '{0}', defaulting to install...", mode);
					return Install();
			}
		}
	}
}