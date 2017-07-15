using System;
using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.Plugins
{
	/// <summary>
	///     Describes a plugin that is located inside a .NET assembly.
	/// </summary>
	public sealed class PluginDescription
		: IPluginDescription
	{
		public string FilePath { get; set; }
		public string Author { get; set; }
		public string Description { get; set; }
		public Uri Website { get; set; }
		public IReadOnlyDictionary<Type, string> Plugins { get; set; }
	}
}