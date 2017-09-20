using System;
using System.Diagnostics.Contracts;
using Tailviewer.Core.Settings;

namespace Tailviewer.Core
{
	/// <summary>
	///     A globally unique identifier for a <see cref="QuickFilter" />.
	/// </summary>
	public struct QuickFilterId
		: IEquatable<QuickFilterId>
	{
		public static readonly QuickFilterId Empty = new QuickFilterId();

		public Guid Value => _value;

		public bool Equals(QuickFilterId other)
		{
			return _value.Equals(other._value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is QuickFilterId && Equals((QuickFilterId) obj);
		}

		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		public static bool operator ==(QuickFilterId left, QuickFilterId right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(QuickFilterId left, QuickFilterId right)
		{
			return !left.Equals(right);
		}

		private readonly Guid _value;

		public QuickFilterId(Guid value)
		{
			_value = value;
		}

		public override string ToString()
		{
			return _value.ToString();
		}

		[Pure]
		public static QuickFilterId CreateNew()
		{
			return new QuickFilterId(Guid.NewGuid());
		}
	}
}