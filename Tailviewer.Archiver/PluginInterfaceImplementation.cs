using System.Runtime.Serialization;

namespace Tailviewer.Core.Plugins
{
	[DataContract]
	public struct PluginInterfaceImplementation
	{
		[DataMember]
		public string InterfaceTypename { get; set; }

		[DataMember]
		public string ImplementationTypename { get; set; }
	}
}