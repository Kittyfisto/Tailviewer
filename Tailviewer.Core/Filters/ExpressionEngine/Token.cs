using System;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal struct Token : IEquatable<Token>
	{
		public readonly TokenType Type;
		public readonly string Value;

		public Token(TokenType type, string value = null)
		{
			Type = type;
			Value = value;
		}

		public override string ToString()
		{
			if (!string.IsNullOrEmpty(Value))
				return string.Format("{0}: '{1}'", Type, Value);

			return Type.ToString();
		}

		public bool Equals(Token other)
		{
			return string.Equals(Value, other.Value) && Type == other.Type;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Token && Equals((Token) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Value != null ? Value.GetHashCode() : 0)*397) ^ (int) Type;
			}
		}

		public static bool operator ==(Token left, Token right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Token left, Token right)
		{
			return !left.Equals(right);
		}
	}
}