using System;
using System.Diagnostics.Contracts;

namespace Tailviewer.Core
{
	/// <summary>
	///     A globally unique identifier for an analysis.
	/// </summary>
	public struct AnalysisId
		: IEquatable<AnalysisId>
	{
		public static readonly AnalysisId Empty = new AnalysisId();

		public AnalysisId(Guid value)
		{
			_value = value;
		}

		public Guid Value => _value;

		public bool Equals(AnalysisId other)
		{
			return _value.Equals(other._value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is AnalysisId && Equals((AnalysisId) obj);
		}

		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		public static bool operator ==(AnalysisId left, AnalysisId right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(AnalysisId left, AnalysisId right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return _value.ToString();
		}

		private readonly Guid _value;

		[Pure]
		public static AnalysisId CreateNew()
		{
			return new AnalysisId(Guid.NewGuid());
		}
	}
}