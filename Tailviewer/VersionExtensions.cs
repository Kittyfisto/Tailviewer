using System;

namespace Tailviewer
{
	public static class VersionExtensions
	{
		public static string Format(this Version version)
		{
			var value = version.ToString(3);
			return value;
		}
	}
}