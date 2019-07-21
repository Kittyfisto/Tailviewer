using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Tailviewer.Archiver.Plugins
{
	public interface IPluginArchive
		: IDisposable
	{
		IPluginPackageIndex Index { get; }

		/// <summary>
		///     Returns a stream to read the icon of this archive from.
		/// </summary>
		/// <returns>A stream pointing towards the icon of this plugin or null the plugin doesn't have an icon</returns>
		Stream ReadIcon();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="entryName"></param>
		/// <returns></returns>
		Assembly LoadAssembly(string entryName);

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		Assembly LoadPlugin();

		/// <summary>
		///   
		/// </summary>
		IReadOnlyList<SerializableChange> LoadChanges();
	}
}