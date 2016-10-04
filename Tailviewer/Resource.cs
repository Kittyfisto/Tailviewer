using System;
using System.IO;
using System.Windows;

namespace Tailviewer
{
	public static class Resource
	{
		public static Stream OpenResource(string relativeResourceName)
		{
			var path = string.Format("pack://application:,,,/Tailviewer;component/{0}", relativeResourceName);
			var uri = new Uri(path, UriKind.Absolute);
			return OpenResource(uri);
		}

		public static Stream OpenResource(Uri uri)
		{
			var resource = Application.GetResourceStream(uri);
			if (resource == null)
				throw new Exception(string.Format("Unable to find '{0}'", uri));

			return resource.Stream;
		}

		public static string ReadResourceToEnd(string relativeResourceName)
		{
			using (var stream = OpenResource(relativeResourceName))
			using (var reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}
	}
}