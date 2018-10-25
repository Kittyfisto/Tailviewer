using System;

namespace Tailviewer.Archiver.Plugins
{
	sealed class PluginStatus
		: IPluginStatus
	{
		public bool IsInstalled { get; set; }

		public bool IsLoaded { get; set; }

		public Exception LoadException { get; set; }
	}
}