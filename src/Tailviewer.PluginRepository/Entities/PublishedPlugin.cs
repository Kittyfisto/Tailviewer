using System.Collections.Generic;
using System.Runtime.Serialization;
using Tailviewer.Archiver.Repository;

namespace Tailviewer.PluginRepository.Entities
{
	[DataContract]
	public class PublishedPlugin
	{
		/// <summary>
		///     The identifier (id + version) of the plugin.
		/// </summary>
		[DataMember]
		public PluginIdentifier Identifier { get; set; }

		/// <summary>
		///     The user who published the plugin.
		/// </summary>
		[DataMember]
		public string User { get; set; }

		/// <summary>
		///     The list of interfaces required by the plugin.
		/// </summary>
		//[DataMember]
		public List<PluginInterface> RequiredInterfaces { get; set; }
	}
}