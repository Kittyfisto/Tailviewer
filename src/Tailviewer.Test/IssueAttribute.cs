using System;

namespace Tailviewer.Test
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
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