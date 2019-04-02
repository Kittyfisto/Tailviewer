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

		/// <summary>
		/// The version of the interface at the time it was implemented.
		/// </summary>
		[DataMember]
		public int InterfaceVersion { get; set; }
	}
}