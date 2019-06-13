using System.Reflection;
using log4net;

namespace Tailviewer
{
	public static class ArgumentParser
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static Arguments TryParse(string[] args)
		{
			var arguments = new Arguments();
			if (args?.Length == 3)
			{
				switch (args[0])
				{
					case "/testloadplugin":
						arguments.Mode = Modes.TestLoadPlugin;
						arguments.FileToOpen = args[1];
						arguments.PluginInterface = args[2];
						break;

					default:
						Log.WarnFormat("Unknown command line parameter: {0}", args[0]);
						break;
				}
			}
			else if (args?.Length > 0)
			{
				arguments.FileToOpen = args[0];
			}
			else
			{
				arguments.FileToOpen = null;
			}
			return arguments;
		}

		public enum Modes
		{
			Default,

			/// <summary>
			/// Test-load a given plugin.
			/// Tailviewer will only boot up systems required for loading
			/// a plugin, load the specified plugin and then return 0 on success
			/// or any other number on failure. In the latter case,
			/// the log will contain information as to why the plugin failed to be loaded.
			/// </summary>
			TestLoadPlugin
		}

		public sealed class Arguments
		{
			public Modes Mode;
			public string FileToOpen;
			public string PluginInterface;
		}
	}
}