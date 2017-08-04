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

		/// <inheritdoc />
		public int PluginArchiveVersion { get; set; }

		/// <inheritdoc />
		[DataMember]
		public string Id { get; set; }

		/// <inheritdoc />
		[DataMember]
		public string Name { get; set; }

		/// <inheritdoc />
		[DataMember]
		public string Author { get; set; }

		/// <inheritdoc />
		[DataMember]
		public string Description { get; set; }

		/// <inheritdoc />
		[DataMember]
		public string Website { get; set; }

		/// <inheritdoc />
		Version IPluginPackageIndex.Version
		{
			get
			{
				Version version;
				System.Version.TryParse(Version, out version);
				return version;
			}
		}

		[DataMember]
		public string Version { get; set; }

		IEnumerable<PluginInterfaceImplementation> IPluginPackageIndex.ImplementedPluginInterfaces => ImplementedPluginInterfaces;

		IReadOnlyList<IAssemblyDescription> IPluginPackageIndex.Assemblies => Assemblies;

		IReadOnlyList<INativeImageDescription> IPluginPackageIndex.NativeImages => NativeImages;
	}
}