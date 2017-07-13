using System;

namespace Tailviewer.Test
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class EnhancementAttribute
		: Attribute
	{
		public EnhancementAttribute(string defectUri)
		{
			DefectUri = defectUri;
		}

		public string DefectUri { get; set; }
	}
}
