using System;
using System.Diagnostics.Contracts;

namespace SharpTail
{
	public struct Size : IEquatable<Size>
	{
		public static readonly Size Zero;
		public static readonly Size OneByte;
		public static readonly Size OneKilobyte;
		public static readonly Size OneMegabyte;
		public static readonly Size OneGigabyte;
		private readonly ulong _numBytes;

		static Size()
		{
			Zero = new Size();
			OneByte = new Size(1);
			OneKilobyte = new Size(1024);
			OneMegabyte = new Size(1024*1024);
			OneGigabyte = new Size(1024*1024*1024);
		}

		private Size(ulong numBytes)
		{
			_numBytes = numBytes;
		}

		public bool Equals(Size other)
		{
			return _numBytes == other._numBytes;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Size && Equals((Size) obj);
		}

		public override int GetHashCode()
		{
			return _numBytes.GetHashCode();
		}

		public static bool operator >(Size lhs, Size rhs)
		{
			return lhs._numBytes > rhs._numBytes;
		}

		public static bool operator <(Size lhs, Size rhs)
		{
			return lhs._numBytes < rhs._numBytes;
		}

		public static bool operator >=(Size lhs, Size rhs)
		{
			return lhs._numBytes >= rhs._numBytes;
		}

		public static bool operator <=(Size lhs, Size rhs)
		{
			return lhs._numBytes <= rhs._numBytes;
		}

		public static bool operator ==(Size lhs, Size rhs)
		{
			return lhs._numBytes == rhs._numBytes;
		}

		public static bool operator !=(Size lhs, Size rhs)
		{
			return lhs._numBytes != rhs._numBytes;
		}

		public static double operator /(Size lhs, Size rhs)
		{
			return 1.0*lhs._numBytes/rhs._numBytes;
		}

		public override string ToString()
		{
			if (this > OneGigabyte)
			{
				return string.Format("{0:F2} Gb", this/OneGigabyte);
			}
			if (this > OneMegabyte)
			{
				return string.Format("{0:F2} Mb", this/OneMegabyte);
			}
			if (this > OneKilobyte)
			{
				return string.Format("{0:F2} Kb", this/OneKilobyte);
			}

			return string.Format("{0} bytes", _numBytes);
		}

		[Pure]
		public static Size FromBytes(long numBytes)
		{
			return new Size((ulong) numBytes);
		}
	}
}