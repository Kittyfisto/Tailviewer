using System;
using System.Diagnostics.Contracts;
using Tailviewer.Api;

namespace Tailviewer.BusinessLogic.Highlighters
{
	/// <summary>
	///     A globally unique identifier for a highlighter.
	/// </summary>
	public struct HighlighterId
		: IEquatable<HighlighterId>
		, ISerializableType
	{
		/// <summary>
		///     The value for an empty id, representing nothing.
		/// </summary>
		public static readonly HighlighterId Empty = new HighlighterId();

		/// <summary>
		///     The underlying value of this id.
		/// </summary>
		public Guid Value => _value;

		/// <inheritdoc />
		public bool Equals(HighlighterId other)
		{
			return _value.Equals(other._value);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is HighlighterId && Equals((HighlighterId) obj);
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
		public static bool operator ==(HighlighterId left, HighlighterId right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares the two values for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(HighlighterId left, HighlighterId right)
		{
			return !left.Equals(right);
		}

		private Guid _value;

		/// <summary>
		///     Initializes this id.
		/// </summary>
		/// <param name="value"></param>
		public HighlighterId(Guid value)
		{
			_value = value;
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
		public static HighlighterId CreateNew()
		{
			return new HighlighterId(Guid.NewGuid());
		}

		/// <inheritdoc />
		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("value", _value);
		}

		/// <inheritdoc />
		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("value", out _value);
		}
	}
}
