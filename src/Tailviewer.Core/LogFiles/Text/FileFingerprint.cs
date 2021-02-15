using System;
using System.IO;

namespace Tailviewer.Core.LogFiles.Text
{
	internal sealed class FileFingerprint
	{
		#region Equality members

		private bool Equals(FileFingerprint other)
		{
			return _created.Equals(other._created) && _modified.Equals(other._modified) && _size == other._size;
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
				hashCode = (hashCode * 397) ^ _modified.GetHashCode();
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
		private readonly DateTime _modified;
		private readonly long _size;

		public FileFingerprint(DateTime created, DateTime modified, long size)
		{
			_created = created;
			_modified = modified;
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
			return $"Size: {_size}, Created: {_created}, Modified: {_modified}";
		}

		#endregion
	}
}