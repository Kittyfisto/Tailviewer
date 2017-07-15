using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.Plugins
{
	public interface IPluginScanner
	{
		/// <summary>
		///     Finds all plugins in the given directory path (recursive).
		///     Plugins are .NET assemblies compiled against AnyCPU that implement at least
		///     one of the available <see cref="IPlugin" /> interfaces.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		IReadOnlyList<IPluginDescription> ReflectPlugins(string path);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pluginPath"></param>
		/// <returns></returns>
		IPluginDescription ReflectPlugin(string pluginPath);
	}
}