using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Archiver.Repository;

namespace Tailviewer.PluginRepository.Entities
{
	/// <summary>
	///     Describes a plugin in the repository's database.
	/// </summary>
	/// <remarks>
	///     Making breaking changes to this type is not advised because it will
	///     break existing repositories.
	/// </remarks>
	[DataContract]
	public class PublishedPlugin
	{
		public PublishedPlugin()
		{
		}

		public PublishedPlugin(IPluginPackageIndex pluginIndex)
		{
			Name = pluginIndex.Name;
			Author = pluginIndex.Author;
			Website = pluginIndex.Website;
			Description = pluginIndex.Description;
			RequiredInterfaces = pluginIndex.ImplementedPluginInterfaces
			                                .Select(x => new PluginInterface(x.InterfaceTypename, x.InterfaceVersion))
			                                .ToList();
		}

		/// <summary>
		///     The identifier (id + version) of the plugin.
		/// </summary>
		[DataMember]
		public PluginIdentifier Identifier { get; set; }

		/// <summary>
		///     The username of the person to add this plugin to the repository.
		/// </summary>
		[DataMember]
		public string Publisher { get; set; }

		/// <summary>
		///     The name of the plugin.
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		///     The author of the plugin.
		/// </summary>
		[DataMember]
		public string Author { get; set; }

		/// <summary>
		///     The uri to the website of the plugin.
		/// </summary>
		[DataMember]
		public string Website { get; set; }

		/// <summary>
		///     Human readable description of the plugin.
		/// </summary>
		[DataMember]
		public string Description { get; set; }

		/// <summary>
		///     The size of the plugin in bytes.
		/// </summary>
		[DataMember]
		public long SizeInBytes { get; set; }

		/// <summary>
		///     The date the plugin was built.
		/// </summary>
		/// <remarks>
		///     Is determined from the linker timestamp in the PE header of the plugin's assembly.
		/// </remarks>
		[DataMember]
		public DateTime BuildDate { get; set; }

		/// <summary>
		///     The date the plugin was published (i.e. added) in this repository.
		/// </summary>
		[DataMember]
		public DateTime PublishDate { get; set; }

		/// <summary>
		///     The list of interfaces required by the plugin.
		/// </summary>
		[DataMember]
		public List<PluginInterface> RequiredInterfaces { get; set; }
	}
}