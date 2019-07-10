using System.Collections.Generic;
using System.Runtime.Serialization;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Archiver.Registry;

namespace Tailviewer.PluginRepository
{
	[DataContract]
	public sealed class PluginRequirements
	{
		public PluginRequirements()
		{}

		public PluginRequirements(IEnumerable<PluginInterfaceImplementation> interfaces)
		{
			RequiredInterfaces = new List<PluginInterface>();
			foreach (var @interface in interfaces)
			{
				RequiredInterfaces.Add(new PluginInterface(@interface.InterfaceTypename, @interface.InterfaceVersion));
			}
		}

		[DataMember]
		public List<PluginInterface> RequiredInterfaces { get; set; }
	}
}