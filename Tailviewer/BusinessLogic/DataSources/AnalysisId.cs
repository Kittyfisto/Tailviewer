using System;
using System.Diagnostics.Contracts;

namespace Tailviewer.BusinessLogic.DataSources
{
	public struct AnalysisId
		: IEquatable<AnalysisId>
	{
		public AnalysisId(Guid id)
		{
			_id = id;
		}

		public bool Equals(AnalysisId other)
		{
			return _id.Equals(other._id);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is AnalysisId && Equals((AnalysisId) obj);
		}

		public override int GetHashCode()
		{
			return _id.GetHashCode();
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
			return _id.ToString();
		}

		private readonly Guid _id;

		[Pure]
		public static AnalysisId CreateNew()
		{
			return new AnalysisId(Guid.NewGuid());
		}
	}
}