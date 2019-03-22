using System;

namespace Tailviewer.Test
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class IssueAttribute
		: Attribute
	{
		public IssueAttribute(string issueUri)
		{
			IssueUri = issueUri;
		}

		public string IssueUri { get; set; }
	}
}