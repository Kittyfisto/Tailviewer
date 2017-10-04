using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Tailviewer.BusinessLogic.Filesystem
{
	/// <summary>
	///     Interface to interact with the filesystem.
	///     Each and every call is implemented non-blocking.
	/// </summary>
	public interface IFilesystem
	{
		#region Directories

		/// <summary>
		///     Creates the given directory if it doesn't exist yet.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Task<IDirectoryInfo> CreateDirectory(string path);

		/// <summary>
		///     Deletes an empty directory from a specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Task DeleteDirectory(string path);

		/// <summary>
		///     Deletes the specified directory and, if indicated, any subdirectories and files
		///     in the directory.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="recursive"></param>
		/// <returns></returns>
		Task DeleteDirectory(string path, bool recursive);

		/// <summary>
		///     Tests if a directory with the given path exists.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Task<bool> DirectoryExists(string path);

		/// <summary>
		///     Obtains information about the given directory.
		/// </summary>
		/// <param name="directoryName"></param>
		/// <returns></returns>
		Task<IDirectoryInfo> GetDirectoryInfo(string directoryName);

		/// <summary>
		///     Returns an enumerable collection of file names in a specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Task<IEnumerable<string>> EnumerateFiles(string path);

		/// <summary>
		///     Returns an enumerable collection of file names in a specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="searchPattern"></param>
		/// <returns></returns>
		Task<IEnumerable<string>> EnumerateFiles(string path, string searchPattern);

		/// <summary>
		///     Returns an enumerable collection of file names in a specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="searchPattern"></param>
		/// <param name="searchOption"></param>
		/// <returns></returns>
		Task<IEnumerable<string>> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);

		/// <summary>
		///     Returns an enumerable collection of directory names in a specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Task<IEnumerable<string>> EnumerateDirectories(string path);

		/// <summary>
		///     Returns an enumerable collection of directory names in a specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="searchPattern"></param>
		/// <returns></returns>
		Task<IEnumerable<string>> EnumerateDirectories(string path, string searchPattern);

		/// <summary>
		///     Returns an enumerable collection of directory names in a specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="searchPattern"></param>
		/// <param name="searchOption"></param>
		/// <returns></returns>
		Task<IEnumerable<string>> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption);

		#endregion

		#region Files

		/// <summary>
		///     Obtains information about the given file.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		Task<IFileInfo> GetFileInfo(string fileName);

		/// <summary>
		///     Tests if a file with the given path exists.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Task<bool> FileExists(string path);

		/// <summary>
		///     Writes the given data to the given file.
		/// </summary>
		/// <remarks>
		///     This method copies the given buffer before writing to the file on the I/O thread.
		/// </remarks>
		/// <param name="path"></param>
		/// <param name="bytes"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> or <paramref name="bytes" /> is null</exception>
		Task WriteAllBytes(string path, byte[] bytes);

		/// <summary>
		///     Deletes the specified file.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Task DeleteFile(string path);

		#endregion
	}
}