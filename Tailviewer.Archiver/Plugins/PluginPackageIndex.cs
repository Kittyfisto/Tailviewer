using System;
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
		///     The list of native images contained in the plugin package.
		/// </summary>
		[DataMember]
		public List<NativeImageDescription> NativeImages { get; set; }

		/// <summary>
		/// </summary>
		[DataMember]
		public List<PluginInterfaceImplementation> ImplementedPluginInterfaces { get; set; }

		IEnumerable<PluginInterfaceImplementation> IPluginPackageIndex.ImplementedPluginInterfaces =>
			ImplementedPluginInterfaces;

		/// <summary>
		/// </summary>
		[DataMember]
		public string Author { get; set; }

		/// <summary>
		/// </summary>
		[DataMember]
		public string Description { get; set; }

		/// <summary>
		/// </summary>
		[DataMember]
		public string Website { get; set; }

		/// <inheritdoc />
		public Version Version
		{
			get
			{
				Version version;
				Version.TryParse(SerializablePluginVersion, out version);
				return version;
			}
			set => SerializablePluginVersion = value != null ? value.ToString() : null;
		}

		[DataMember]
		public string SerializablePluginVersion { get; set; }

		IReadOnlyList<IAssemblyDescription> IPluginPackageIndex.Assemblies => Assemblies;

		IReadOnlyList<INativeImageDescription> IPluginPackageIndex.NativeImages => NativeImages;
	}
}