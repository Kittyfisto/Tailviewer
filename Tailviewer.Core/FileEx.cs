using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tailviewer.Core
{
	/// <summary>
	///     "Extension" methods to the static <see cref="File" /> methods.
	/// </summary>
	public static class FileEx
	{
		/// <summary>
		///     Finds the closest folder to the given directory or file path.
		/// </summary>
		/// <example>
		///     If <paramref name="path" /> is C:\foo\bar.txt and bar.txt does not exist, but
		///     C:\foo does, then C:\foo is returned. If C:\foo doesn't exist, then C:\ is returned
		///     (because that drive is mounted).
		/// </example>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string FindClosestExistingFolder(string path)
		{
			string absolutePath;
			var drive = GetDriveLetterFromPath(path, out absolutePath);
			if (drive == null)
				return null;

			foreach (var subPath in GetFolderPaths(path))
				if (Directory.Exists(subPath))
					return subPath;

			return null;
		}

		/// <summary>
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static IEnumerable<string> GetFolderPaths(string path)
		{
			if (string.IsNullOrEmpty(path))
				return Enumerable.Empty<string>();

			path = path.Replace(oldChar: '/', newChar: '\\');

			var ret = new List<string> {path};
			var index = path.Length - 1;
			var count = path.Length;
			while ((index = path.LastIndexOf(value: '\\', startIndex: index, count: count)) != -1)
			{
				var subPath = path.Substring(startIndex: 0, length: index + 1);
				if (ret.Count > 1 || !string.Equals(path, subPath))
					ret.Add(subPath);

				index -= 1;
				count = index;
			}
			return ret;
		}

		/// <summary>
		///     Resolves the given path to an absolute one, if it isn't already and then returns the drive letter from the given
		///     path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="absolutePath"></param>
		/// <returns></returns>
		public static char? GetDriveLetterFromPath(string path, out string absolutePath)
		{
			if (string.IsNullOrEmpty(path))
			{
				absolutePath = null;
				return null;
			}

			if (Path.IsPathRooted(path))
				absolutePath = path;
			else
				absolutePath = Path.Combine(Directory.GetCurrentDirectory(), path);

			var drive = path[index: 0];
			return drive;
		}

		/// <summary>
		///     Tests if the given file/directory path exists and is currently reachable.
		///     Does not block (much) longer than the given timeout.
		/// </summary>
		/// <remarks>
		///     TODO: This method should accept paths such as \\servername\foobar as well.
		/// </remarks>
		/// <param name="path"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		public static bool Exists(string path, TimeSpan timeout)
		{
			string absolutePath;
			var drive = GetDriveLetterFromPath(path, out absolutePath);
			if (drive == null)
				return false;

			if (!Drive.IsReachable(drive.Value, timeout))
				return false;

			// Theoretically, we could end up blocking the full 60 seconds
			// if the drive were to become unreachable in between the previous
			// and this statement... So, is it actually worth it or would an async
			// call to Exists be better? I'm torn.
			return File.Exists(absolutePath);
		}
	}
}