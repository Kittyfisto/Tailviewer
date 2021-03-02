using System;
using Tailviewer.Api;

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
		///     The version of the interface being implemented.
		/// </summary>
		PluginInterfaceVersion Version { get; }

		/// <summary>
		/// The <see cref="IPlugin"/> interface which is implemented by this type.
		/// </summary>
		Type InterfaceType { get; }
	}
}