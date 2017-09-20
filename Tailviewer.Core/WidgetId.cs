using System;
using System.Diagnostics.Contracts;

namespace Tailviewer.Core
{
	public struct WidgetId
		: IEquatable<WidgetId>
	{
		public static readonly WidgetId Empty = new WidgetId();

		private readonly Guid _value;

		public WidgetId(Guid value)
		{
			_value = value;
		}

		public Guid Value => _value;

		public bool Equals(WidgetId other)
		{
			return _value.Equals(other._value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is WidgetId && Equals((WidgetId) obj);
		}

		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		public static bool operator ==(WidgetId left, WidgetId right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(WidgetId left, WidgetId right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return _value.ToString();
		}

		[Pure]
		public static WidgetId CreateNew()
		{
			return new WidgetId(Guid.NewGuid());
		}
	}
}
