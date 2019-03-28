using System.Runtime.Serialization;

namespace Tailviewer.Archiver.Plugins
{
	[DataContract]
	public struct SerializableTypeDescription
	{
		/// <summary>
		/// The name with which the serializable type is referred to during (de)serialization.
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// The full .NET name of the serializable type.
		/// </summary>
		[DataMember]
		public string FullName { get; set; }
	}
}