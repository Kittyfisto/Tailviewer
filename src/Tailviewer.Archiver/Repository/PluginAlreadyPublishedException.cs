using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Tailviewer.Archiver.Repository
{
	[Serializable]
	public class PluginAlreadyPublishedException
		: Exception
	{
		public PluginAlreadyPublishedException()
			: base("The given plugin has already been published in that version and cannot be modified.")
		{
		}

		public PluginAlreadyPublishedException(string message)
			: base(message)
		{
		}

		public PluginAlreadyPublishedException(string message, Exception inner)
			: base(message, inner)
		{
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected PluginAlreadyPublishedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}
	}
}