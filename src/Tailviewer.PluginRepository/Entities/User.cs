using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
