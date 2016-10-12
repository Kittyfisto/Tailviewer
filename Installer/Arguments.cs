namespace Installer
{
	public sealed class Arguments
	{
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

			switch (args[0])
			{
				case "update":
					return Update(args[1]);

				case "silentinstall":
					return SilentInstall(args[1]);

				default:
					return Install();
			}
		}
	}
}