using System;
using System.Diagnostics.Contracts;

{
	/// <summary>
	///     A globally unique identifier for a log analyser instance.
	/// </summary>
	public struct LogAnalyserId
		: IEquatable<LogAnalyserId>
	{
		public static readonly LogAnalyserId Empty = new LogAnalyserId();

		private readonly Guid _value;

		public LogAnalyserId(Guid value)
		{
			_value = value;
		}

		public Guid Value => _value;

		public bool Equals(LogAnalyserId other)
		{
			return Value.Equals(other.Value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is LogAnalyserId && Equals((LogAnalyserId) obj);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public static bool operator ==(LogAnalyserId left, LogAnalyserId right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LogAnalyserId left, LogAnalyserId right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return _value.ToString();
		}

		[Pure]
		public static LogAnalyserId CreateNew()
		{
			return new LogAnalyserId(Guid.NewGuid());
		}
	}
}