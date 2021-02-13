using System;
using System.IO;

namespace Tailviewer.Test
{
	public static class PathEx
	{
		/// <summary>
		/// Actually returns a temporary file, unlike <see cref="Path.GetTempFileName"/> which may return a new temporary file
		/// or just given up because it's the weekend and it's already in its pajamas.
		/// </summary>
		/// <returns></returns>
		public static string GetTempFileName()
		{
			var path = Path.Combine(Path.GetTempPath(), "Tailviewer", "Tests");
			Directory.CreateDirectory(path);

			var fileName = Guid.NewGuid().ToString();
			return Path.Combine(path, fileName);
		}
	}
}