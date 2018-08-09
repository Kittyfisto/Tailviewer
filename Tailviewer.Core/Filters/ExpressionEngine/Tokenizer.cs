using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class Tokenizer
	{
		private static readonly IReadOnlyDictionary<TokenType, string> SpecialTokens;

		static Tokenizer()
		{
			SpecialTokens = new Dictionary<TokenType, string>
			{
				{TokenType.OpenBracket, "("},
				{TokenType.CloseBracket, ")"},
				{TokenType.LessOrEquals, "<="},
				{TokenType.GreaterOrEquals, ">="},
				{TokenType.GreaterThan, ">"},
				{TokenType.LessThan, "<"},
				{TokenType.Equals, "=="},
				{TokenType.NotEquals, "!="},
				{TokenType.Not, "!"},
				{TokenType.And, "and"},
				{TokenType.Or, "or"},
				{TokenType.Contains, "contains"},
				{TokenType.Is, "is"},
				{TokenType.Quotation, "'"},
				{TokenType.Dollar, "$"},
				{TokenType.True, "true"},
				{TokenType.False, "false"}
			};
		}

		[Pure]
		public static string ToString(TokenType token)
		{
			SpecialTokens.TryGetValue(token, out var value);
			return value;
		}

		[Pure]
		public List<Token> Tokenize(string expression)
		{
			var tokens = new List<Token>();
			if (expression != null)
			{
				for (int i = 0; i < expression.Length; )
				{
					Token token;
					if (!Match(ref i, expression, out token))
						throw new ParseException(string.Format("Unable to parse: {0}", expression.Substring(i)));

					tokens.Add(token);
				}
			}
			return tokens;
		}

		private bool Match(ref int startIndex, string expression, out Token token)
		{
			int length;
			if (StartsWithWhitespace(expression, startIndex, out length))
			{
				token = new Token(TokenType.Whitespace, expression.Substring(startIndex, length));
				startIndex += length;
				return true;
			}

			foreach (var pair in SpecialTokens)
			{
				if (StartsWith(expression, startIndex, pair.Value))
				{
					startIndex += pair.Value.Length;
					token = new Token(pair.Key);
					return true;
				}
			}

			// => Literal, but we need to determine the extent.
			int i;
			for (i = startIndex; i < expression.Length; ++i)
			{
				if (char.IsWhiteSpace(expression[i]))
					break;

				foreach (var pair in SpecialTokens)
				{
					if (StartsWith(expression, i, pair.Value))
						goto eol;
				}
			}
			eol:

			var value = expression.Substring(startIndex, i - startIndex);
			startIndex += value.Length;
			token = new Token(TokenType.Literal, value);
			return true;
		}

		private static bool StartsWithWhitespace(string expression, int startIndex, out int length)
		{
			length = -1;
			if (expression.Length == 0)
			{
				return false;
			}

			int i;
			for (i = startIndex; i < expression.Length; ++i)
			{
				if (!char.IsWhiteSpace(expression[i]))
					break;
			}

			length = i - startIndex;
			if (length > 0)
				return true;

			return false;
		}

		[Pure]
		private static bool StartsWith(string expression, int startIndex, string token)
		{
			if (expression.Length - startIndex < token.Length)
				return false;

// ReSharper disable LoopCanBeConvertedToQuery
			for (int i = 0; i < token.Length; ++i)
// ReSharper restore LoopCanBeConvertedToQuery
			{
				if (expression[startIndex + i] != token[i])
					return false;
			}

			return true;
		}
	}
}