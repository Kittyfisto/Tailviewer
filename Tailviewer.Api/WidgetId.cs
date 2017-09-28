using System;
using System.Diagnostics.Contracts;
using Tailviewer.Ui.Analysis;

namespace Tailviewer
{
	/// <summary>
	///     A globally unique identifier for a <see cref="IWidgetViewModel"/>.
	/// </summary>
	public struct WidgetId
		: IEquatable<WidgetId>
	{
		/// <summary>
		///     The value for an empty id, representing nothing.
		/// </summary>
		public static readonly WidgetId Empty = new WidgetId();

		private readonly Guid _value;

		/// <summary>
		///     Initializes this id.
		/// </summary>
		/// <param name="value"></param>
		public WidgetId(Guid value)
		{
			_value = value;
		}

		/// <summary>
		///     The underlying value of this id.
		/// </summary>
		public Guid Value => _value;

		/// <inheritdoc />
		public bool Equals(WidgetId other)
		{
			return _value.Equals(other._value);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is WidgetId && Equals((WidgetId) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		/// <summary>
		///     Compares the two values for equality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(WidgetId left, WidgetId right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares the two values for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(WidgetId left, WidgetId right)
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
		public static WidgetId CreateNew()
		{
			return new WidgetId(Guid.NewGuid());
		}
	}
}
