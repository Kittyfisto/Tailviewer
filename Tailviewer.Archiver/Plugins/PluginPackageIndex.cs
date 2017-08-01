using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	///     Describes the content of a plugin package to tailviewer.
	/// </summary>
	/// <remarks>
	///     Plugin authors do not need to use this class.
	/// </remarks>
	[DataContract]
	public sealed class PluginPackageIndex : IPluginPackageIndex
	{
		/// <summary>
		///     The list of assemblies contained in the plugin package.
		/// </summary>
		[DataMember]
		public List<AssemblyDescription> Assemblies { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public List<PluginInterfaceImplementation> ImplementedPluginInterfaces { get; set; }

		IEnumerable<PluginInterfaceImplementation> IPluginPackageIndex.ImplementedPluginInterfaces => ImplementedPluginInterfaces;

		/// <summary>
		/// </summary>
		[DataMember]
		public string PluginAuthor { get; set; }

		/// <summary>
		/// </summary>
		[DataMember]
		public string PluginDescription { get; set; }

		/// <summary>
		/// </summary>
		[DataMember]
		public string PluginWebsite { get; set; }

		IReadOnlyList<IAssemblyDescription> IPluginPackageIndex.Assemblies => Assemblies;
	}
}