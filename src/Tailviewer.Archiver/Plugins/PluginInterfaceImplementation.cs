using System.Runtime.Serialization;

namespace Tailviewer.Archiver.Plugins
{
	[DataContract]
	public struct PluginInterfaceImplementation
	{
		[DataMember]
		public string InterfaceTypename { get; set; }

		[DataMember]
		public string ImplementationTypename { get; set; }

		[DataMember]
		public int InterfaceVersion { get; set; }
	}
}