using System;
using System.IO;
using System.Reflection;

namespace Tailviewer
{
	public static class AssemblyExtensions
	{
		public static string GetFolder(this Assembly assembly)
		{
			string codeBase = assembly.CodeBase;
			var uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			return Path.GetDirectoryName(path);
		}
	}
}