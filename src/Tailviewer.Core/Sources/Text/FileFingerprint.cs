using System;
using System.IO;

namespace Tailviewer.Core.Sources.Text
{
	internal sealed class FileFingerprint
	{
		#region Equality members

		private bool Equals(FileFingerprint other)
		{
			return _created.Equals(other._created) && _lastModified.Equals(other._lastModified) && _size == other._size;
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is FileFingerprint other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = _created.GetHashCode();
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

		private readonly DateTime _created;

		public DateTime Created
		{
			get { return _created; }
		}

		private readonly DateTime _lastModified;

		public DateTime LastModified
		{
			get { return _lastModified; }
		}

		private readonly long _size;

		public long Size
		{
			get { return _size; }
		}

		public FileFingerprint(DateTime created, DateTime lastModified, long size)
		{
			_created = created;
			_lastModified = lastModified;
			_size = size;
		}

		public static FileFingerprint FromFile(string fileName)
		{
			var info = new FileInfo(fileName);
			return new FileFingerprint(info.CreationTimeUtc, info.LastWriteTimeUtc, info.Length);
		}

		#region Overrides of Object

		public override string ToString()
		{
			return $"Size: {_size}, Created: {_created}, Modified: {_lastModified}";
		}

		#endregion
	}
}