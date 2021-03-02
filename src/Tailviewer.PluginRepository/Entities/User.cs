using System;
using System.Runtime.Serialization;

namespace Tailviewer.PluginRepository.Entities
{
	[DataContract]
	public sealed class User
	{
		[DataMember]
		public string Username { get; set; }

		[DataMember]
		public string Email { get; set; }

		[DataMember]
		public Guid AccessToken { get; set; }
	}
}
