using System;
using System.IO;
using Tailviewer.Api;

namespace Tailviewer.Core.Sources.Text
{
	/// <summary>
	///     This class is responsible for holding a "fingerprint" of a file which can be used to detect if a file has been
	///     modified over time:
	///     Simply compare two fingerprint objects and see if they're equal.
	/// </summary>
	/// <remarks>
	///     This class, by design, doesn't take the actual contents of the file into account. The goal is to be able to perform
	///     a really quick check to
	///     see if it is worth to go and check the contents of the file, i.e this is deliberately split into two parts, one
	///     part being handled by this class,
	///     the other by an <see cref="ILogSource" /> implementation...
	/// </remarks>
	internal sealed class FileFingerprint
	{
		private readonly string _canonicalFilePath;
		private readonly DateTime _created;
		private readonly DateTime _lastModified;
		private readonly long _size;

		public FileFingerprint(string filePath, DateTime created, DateTime lastModified, long size)
		{
			_canonicalFilePath = NormalizePath(filePath);
			_created = created;
			_lastModified = lastModified;
			_size = size;
		}

		public DateTime Created
		{
			get { return _created; }
		}

		public DateTime LastModified
		{
			get { return _lastModified; }
		}

		public long Size
		{
			get { return _size; }
		}

		public static FileFingerprint FromFile(string fileName)
		{
			var info = new FileInfo(fileName);
			return new FileFingerprint(fileName, info.CreationTimeUtc, info.LastWriteTimeUtc, info.Length);
		}

		#region Overrides of Object

		public override string ToString()
		{
			return $"Size: {Metrolib.Size.FromBytes(_size)}, Created: {_created}, Modified: {_lastModified}";
		}

		#endregion

		private static string NormalizePath(string path)
		{
			var absolutePath = Path.IsPathRooted(path)
				? path
				: Path.Combine(Directory.GetCurrentDirectory(), path);

			return Path.GetFullPath(new Uri(absolutePath).LocalPath)
			           .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
			           .ToUpperInvariant();
		}

		#region Equality members

		private bool Equals(FileFingerprint other)
		{
			return _canonicalFilePath == other._canonicalFilePath && _created.Equals(other._created) &&
			       _lastModified.Equals(other._lastModified) && _size == other._size;
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is FileFingerprint other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = _canonicalFilePath != null ? _canonicalFilePath.GetHashCode() : 0;
				hashCode = (hashCode * 397) ^ _created.GetHashCode();
				hashCode = (hashCode * 397) ^ _lastModified.GetHashCode();
				hashCode = (hashCode * 397) ^ _size.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(FileFingerprint left, FileFingerprint right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(FileFingerprint left, FileFingerprint right)
		{
			return !Equals(left, right);
		}

		#endregion
	}
}