using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tailviewer.BusinessLogic
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
				if (!string.Equals(@"\\", subPath) && (ret.Count > 1 || !string.Equals(path, subPath)))
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

			string root = Path.GetPathRoot(path);
			if (root.StartsWith(@"\\"))
			{
				// handles UNS directories
				absolutePath = path;
				return null;
			}
			else if (root.StartsWith("\\"))
			{
				// handles directories off current drive root
				absolutePath = Path.Combine(Directory.GetCurrentDirectory(), path);
				return absolutePath[0];
			}
			else
			{
				absolutePath = path;
			}
			var drive = absolutePath[0];
			return drive;
		}

		/// <summary>
		///     Tests if the given file/directory path exists and is currently reachable.
		///     Does not block (much) longer than the given timeout.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		public static bool Exists(string path, TimeSpan timeout)
		{
			string absolutePath;
			var drive = GetDriveLetterFromPath(path, out absolutePath);

			if (absolutePath == null)
				return false;

			if (drive != null && !Drive.IsReachable(drive.Value, timeout))
				return false;

			// Theoretically, we could end up blocking the full 60 seconds
			// if the drive were to become unreachable in between the previous
			// and this statement... So, is it actually worth it or would an async
			// call to Exists be better? I'm torn.
			return File.Exists(absolutePath);
		}
	}
}