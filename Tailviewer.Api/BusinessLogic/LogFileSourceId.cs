using System;
using System.Runtime.InteropServices;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic
{
	/// <summary>
	///     An id which represents the source of a particular <see cref="LogLine" />.
	/// </summary>
	/// <remarks>
	///     If you're implementing <see cref="ILogFile" />, then you may simply specify <see cref="Default" />.
	/// </remarks>
	/// <remarks>
	///     Currently, this is only used for a merged data source to identify from which original data source
	///     the log line emerged. May be used in other scenarios as well.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LogLineSourceId
		: IEquatable<LogLineSourceId>
	{
		/// <inheritdoc />
		public bool Equals(LogLineSourceId other)
		{
			return _value == other._value;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is LogLineSourceId && Equals((LogLineSourceId) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		/// <summary>
		///     Compares the two ids for equality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(LogLineSourceId left, LogLineSourceId right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares the two ids for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(LogLineSourceId left, LogLineSourceId right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		///     A value which represents that the log line originated from the sender
		/// </summary>
		public static readonly LogLineSourceId Default = new LogLineSourceId(value: 0);

		/// <summary>
		///     A value which represents an invalid source (or a source which cannot be known).
		/// </summary>
		public static readonly LogLineSourceId Invalid = new LogLineSourceId(byte.MaxValue);

		private readonly byte _value;

		/// <summary>
		/// </summary>
		/// <param name="value"></param>
		public LogLineSourceId(byte value)
		{
			_value = value;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			if (this == Invalid)
				return "Invalid";

			return string.Format("#{0}", _value);
		}
	}
}