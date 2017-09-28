using System;
using System.IO;
using System.Reflection;

namespace Tailviewer.Core
{
	/// <summary>
	///     Extension methods for the <see cref="Assembly" /> class.
	/// </summary>
	public static class AssemblyExtensions
	{
		/// <summary>
		///     Returns the folder this assembly is located in.
		/// </summary>
		/// <param name="assembly"></param>
		/// <returns></returns>
		public static string GetFolder(this Assembly assembly)
		{
			var codeBase = assembly.CodeBase;
			var uri = new UriBuilder(codeBase);
			var path = Uri.UnescapeDataString(uri.Path);
			return Path.GetDirectoryName(path);
		}
	}
}