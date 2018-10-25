using System;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	/// 
	/// </summary>
	public interface IPluginStatus
	{
		/// <summary>
		/// 
		/// </summary>
		bool IsInstalled { get; }

		/// <summary>
		/// 
		/// </summary>
		bool IsLoaded { get; }

		/// <summary>
		/// 
		/// </summary>
		Exception LoadException { get; }
	}
}