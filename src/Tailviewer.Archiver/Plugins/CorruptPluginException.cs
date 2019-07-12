using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	///     This exception is thrown when a plugin has been corrupted by the dark
	///     side and deals in absolutes.
	/// </summary>
	[Serializable]
	public class CorruptPluginException
		: Exception
	{
		private const string DefaultMessage = "The plugin is of an unknown format or may be corrupted";
		
		public CorruptPluginException()
			: base(DefaultMessage)
		{
		}

		public CorruptPluginException(Exception inner)
			: base(DefaultMessage, inner)
		{
		}

		public CorruptPluginException(string message)
			: base(message)
		{ }

		public CorruptPluginException(string message, Exception inner)
			: base(message, inner)
		{
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected CorruptPluginException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}
	}
}