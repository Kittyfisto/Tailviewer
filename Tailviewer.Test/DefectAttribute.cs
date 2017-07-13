using System;

namespace Tailviewer.Test
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class DefectAttribute
		: Attribute
	{
		public DefectAttribute(string defectUri)
		{
			DefectUri = defectUri;
		}

		public string DefectUri { get; set; }
	}
}