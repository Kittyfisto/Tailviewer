using System;

namespace Tailviewer.Tests
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
