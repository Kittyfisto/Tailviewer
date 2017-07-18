using System;

namespace Tailviewer.Test
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class EnhancementAttribute
		: Attribute
	{
		public EnhancementAttribute(string ticketUri)
		{
			TicketUri = ticketUri;
		}

		public string TicketUri { get; set; }
	}
}
