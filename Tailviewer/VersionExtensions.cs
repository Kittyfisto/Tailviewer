using System;

namespace Tailviewer
{
	public static class VersionExtensions
	{
		public static string Format(this Version version)
		{
			return version.ToString(3);
		}
	}
}