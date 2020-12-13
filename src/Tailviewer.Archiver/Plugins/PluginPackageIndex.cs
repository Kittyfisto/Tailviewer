using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

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

		[DataMember]
		public List<SerializableTypeDescription> SerializableTypes { get; set; }

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

		Version IPluginPackageIndex.TailviewerApiVersion
		{
			get
			{
				
				Version version;
				System.Version.TryParse(TailviewerApiVersion, out version);
				return version;
			}
		}

		[DataMember]
		public string Version { get; set; }

		[DataMember]
		public string TailviewerApiVersion { get; set; }

		IEnumerable<SerializableTypeDescription> IPluginPackageIndex.SerializableTypes => SerializableTypes;

		IEnumerable<PluginInterfaceImplementation> IPluginPackageIndex.ImplementedPluginInterfaces => ImplementedPluginInterfaces;

		IReadOnlyList<IAssemblyDescription> IPluginPackageIndex.Assemblies => Assemblies;

		IReadOnlyList<INativeImageDescription> IPluginPackageIndex.NativeImages => NativeImages;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stream"></param>
		public void Serialize(Stream stream)
		{
			using (var writer = new StreamWriter(stream, Encoding.UTF8, 4086, true))
			{
				var serializer = new XmlSerializer(typeof(PluginPackageIndex));
				serializer.Serialize(writer, this);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static PluginPackageIndex Deserialize(Stream stream)
		{
			using (var reader = new StreamReader(stream))
			{
				var serializer = new XmlSerializer(typeof(PluginPackageIndex));
				return serializer.Deserialize(reader) as PluginPackageIndex;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public PluginPackageIndex Roundtrip()
		{
			using (var stream = new MemoryStream())
			{
				Serialize(stream);
				stream.Position = 0;
				return Deserialize(stream);
			}
		}
	}
}