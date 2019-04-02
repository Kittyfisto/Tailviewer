using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Archiver.Plugins.Description
{
	/// <summary>
	/// </summary>
	public interface IPluginImplementationDescription
	{
		/// <summary>
		///     The full .NET type name (sans assembly) of the class implementing the plugin interface.
		/// </summary>
		string FullTypeName { get; }

		/// <summary>
		///     The version of the inteface being implemented.
		/// </summary>
		PluginInterfaceVersion Version { get; }
	}
}