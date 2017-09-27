using System;
using System.Diagnostics.Contracts;

namespace Tailviewer.Core
{
	/// <summary>
	///     A globally unique identifier for a log analyser instance.
	/// </summary>
	public struct LogAnalyserId
		: IEquatable<LogAnalyserId>
	{
		/// <summary>
		///     The value for an empty id, representing nothing.
		/// </summary>
		public static readonly LogAnalyserId Empty = new LogAnalyserId();

		private readonly Guid _value;

		/// <summary>
		///     Initializes this id.
		/// </summary>
		/// <param name="value"></param>
		public LogAnalyserId(Guid value)
		{
			_value = value;
		}

		/// <summary>
		///     The underlying value of this id.
		/// </summary>
		public Guid Value => _value;

		/// <inheritdoc />
		public bool Equals(LogAnalyserId other)
		{
			return Value.Equals(other.Value);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is LogAnalyserId && Equals((LogAnalyserId) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		/// <summary>
		///     Compares the two values for equality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(LogAnalyserId left, LogAnalyserId right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares the two values for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(LogAnalyserId left, LogAnalyserId right)
		{
			return !left.Equals(right);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return _value.ToString();
		}

		/// <summary>
		///     Creates a new id that is guarantueed to be globally unique.
		/// </summary>
		/// <returns></returns>
		[Pure]
		public static LogAnalyserId CreateNew()
		{
			return new LogAnalyserId(Guid.NewGuid());
		}
	}
}