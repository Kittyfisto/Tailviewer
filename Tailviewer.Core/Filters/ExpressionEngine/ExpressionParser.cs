using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class ExpressionParser
	{
		private static readonly IReadOnlyDictionary<TokenType, BinaryOperation> BinaryOperations;
		private static readonly IReadOnlyDictionary<TokenType, UnaryOperation> UnaryOperations;

		private readonly Tokenizer _tokenizer;

		static ExpressionParser()
		{
			BinaryOperations = new Dictionary<TokenType, BinaryOperation>
			{
				{TokenType.Contains, BinaryOperation.Contains},
				{TokenType.LessThan, BinaryOperation.LessThan},
				{TokenType.LessOrEquals, BinaryOperation.LessOrEquals}
			};
			UnaryOperations = new Dictionary<TokenType, UnaryOperation>
			{
				{TokenType.Not, UnaryOperation.Not}
			};
		}

		public ExpressionParser()
		{
			_tokenizer = new Tokenizer();
		}

		[Pure]
		public IExpression Parse(string expression)
		{
			try
			{
				List<Token> tokens = _tokenizer.Tokenize(expression);
				IExpression expr = Parse(tokens);
				//IExpression optimized = ExpressionOptimizer.Run(expr);
				//return optimized;
				return expr;
			}
			catch (Exception e)
			{
				throw new ParseException(string.Format("Unable to parse \"{0}\": {1}", expression, e.Message), e);
			}
		}

		private IExpression Parse(IEnumerable<Token> tokens)
		{
			var stack = new List<TokenOrExpression>();
			int highestPrecedence = 0;
			using (IEnumerator<Token> iterator = tokens.GetEnumerator())
			{
				while (iterator.MoveNext())
				{
					Token token = iterator.Current;
					if (token.Type == TokenType.Whitespace)
						continue;

					if (IsOperator(token.Type))
					{
						int precedence = Precedence(token.Type);
						if (precedence < highestPrecedence)
						{
							TryParseLeftToRight(stack);
						}

						stack.Add(token);
						highestPrecedence = precedence;
					}
					else if (token.Type == TokenType.Quotation)
					{
						// Consume everything until the quote closes again...
						var content = new StringBuilder();
						while (iterator.MoveNext() &&
						       iterator.Current.Type != TokenType.Quotation)
						{
							content.Append(token);
						}

						stack.Add(new TokenOrExpression(new StringLiteral(content.ToString())));
					}
					else
					{
						stack.Add(token);
					}
				}
			}

			if (stack.Count == 0)
				return null;

			if (!TryParseLeftToRight(stack))
				throw new ParseException();

			TokenOrExpression tok = stack[0];
			return tok.Expression;
		}

		private IExpression Parse(TokenOrExpression tokenOrExpression)
		{
			if (tokenOrExpression.Expression != null)
				return tokenOrExpression.Expression;

			return Parse(tokenOrExpression.Token);
		}

		private IExpression Parse(Token token)
		{
			var tokens = new List<TokenOrExpression> {token};
			if (!TryParseOne(tokens))
				throw new ParseException(string.Format("Expected token or literal but found: {0}", token));

			return tokens[0].Expression;
		}

		/// <summary>
		///     Parses an expression from the given stack in left-to-right order.
		///     Operator precedences are ignored.
		/// </summary>
		/// <param name="tokens"></param>
		private bool TryParseLeftToRight(List<TokenOrExpression> tokens)
		{
			int beginTokens;
			do
			{
				beginTokens = tokens.Count;
				if (!TryParseOne(tokens))
					return false;

				if (tokens.Count > beginTokens)
					throw new Exception("Parser encountered an error: There shouldn't be more tokens than we started with!");
			} while (tokens.Count < beginTokens);

			if (tokens.Count == 1 &&
			    tokens[0].Expression != null)
				return true;

			return false;
		}

		private bool TryParseOne(List<TokenOrExpression> tokens)
		{
			if (TryParseVariableReference(tokens))
				return true;

			if (TryParseBinaryOperation(tokens))
				return true;

			if (TryParseUnaryOperation(tokens))
				return true;

			if (TryParseLiteral(tokens))
				return true;

			if (tokens.Count >= 1)
			{
				if (tokens[0].Expression != null)
					return true;

				if (tokens[0].Token.Type == TokenType.Literal)
				{
					tokens[0] = new TokenOrExpression(new StringLiteral(tokens[0].Token.Value));
					return true;
				}
			}

			return false;
		}

		private bool TryParseLiteral(List<TokenOrExpression> tokens)
		{
			if (Matches(tokens, TokenType.True))
			{
				tokens[0] = new TokenOrExpression(new TrueExpression());
				return true;
			}

			if (Matches(tokens, TokenType.False))
			{
				tokens[0] = new TokenOrExpression(new FalseExpression());
				return true;
			}

			if (Matches(tokens, TokenType.Literal))
			{
				tokens[0] = new TokenOrExpression(Literal.Create(tokens[0].Token.Value));
				return true;
			}

			return false;
		}

		private bool TryParseVariableReference(List<TokenOrExpression> tokens)
		{
			if (Matches(tokens, TokenType.Dollar, TokenType.Literal))
			{
				TokenOrExpression name = tokens[1];
				tokens.RemoveRange(0, 4);
				tokens.Insert(0, new TokenOrExpression(CreateVariableExpression(name.Token.Value)));
				return true;
			}

			return false;
		}

		private static IExpression CreateVariableExpression(string name)
		{
			switch (name.ToLower())
			{
				case MessageExpression.Value:
					return new MessageExpression();

				case LineExpression.Value:
					return new LineExpression();

				default:
					throw new ParseException(string.Format("Unknown variable: {0}", name));
			}
		}

		private bool TryParseBinaryOperation(List<TokenOrExpression> tokens)
		{
			if (tokens.Count >= 3 && IsBinaryOperator(tokens[1].Token.Type, out var operation))
			{
				// Binary expression
				TokenOrExpression lhs = tokens[0];
				tokens.RemoveRange(0, 2);
				TryParseLeftToRight(tokens);
				if (tokens.Count != 1)
					throw new ParseException();

				IExpression leftHandSide = Parse(lhs);
				IExpression rightHandSide = tokens[0].Expression;

				var expression = BinaryExpression.Create(operation, leftHandSide, rightHandSide);
				tokens.RemoveAt(0);
				tokens.Insert(0, new TokenOrExpression(expression));
				return true;
			}

			return false;
		}

		private bool TryParseUnaryOperation(List<TokenOrExpression> tokens)
		{
			if (tokens.Count >= 2 && IsUnaryOperator(tokens[0].Token.Type, out var operation))
			{
				tokens.RemoveAt(0);
				if (!TryParseLeftToRight(tokens))
					throw new ParseException();
				if (tokens.Count != 1)
					throw new ParseException();

				IExpression rightHandSide = tokens[0].Expression;
				IExpression expression;

				switch (operation)
				{
					case UnaryOperation.Not:
						expression = new NotExpression(rightHandSide);
						break;

					default:
						throw new NotImplementedException(string.Format("Expression not implemented: {0}", operation));
				}

				tokens.RemoveAt(0);
				tokens.Insert(0, new TokenOrExpression(expression));
				return true;
			}

			return false;
		}

		[Pure]
		private static bool IsOperator(TokenType type)
		{
			BinaryOperation unused1;
			if (IsBinaryOperator(type, out unused1))
				return true;

			UnaryOperation unused2;
			if (IsUnaryOperator(type, out unused2))
				return true;

			return false;
		}

		private static bool IsBinaryOperator(TokenType type, out BinaryOperation operation)
		{
			return BinaryOperations.TryGetValue(type, out operation);
		}

		private static bool IsUnaryOperator(TokenType type, out UnaryOperation operation)
		{
			return UnaryOperations.TryGetValue(type, out operation);
		}

		[Pure]
		private static int Precedence(TokenType type)
		{
			switch (type)
			{
				case TokenType.Not:
					return 5;

				case TokenType.LessThan:
				case TokenType.LessOrEquals:
				case TokenType.GreaterThan:
				case TokenType.GreaterOrEquals:
					return 4;

				case TokenType.Equals:
				case TokenType.NotEquals:
					return 3;

				case TokenType.And:
					return 2;

				case TokenType.Or:
					return 1;

				default:
					return 0;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="types"></param>
		/// <returns></returns>
		[Pure]
		private static bool Matches(List<TokenOrExpression> tokens,
		                            params TokenType[] types)
		{
			if (tokens.Count < types.Length)
				return false;

			// ReSharper disable LoopCanBeConvertedToQuery
			for (int i = 0; i < types.Length; ++i)
				// ReSharper restore LoopCanBeConvertedToQuery
			{
				TokenType type = types[i];
				Token token = tokens[i].Token;
				if (token.Type != type)
					return false;
			}

			return true;
		}
	}
}
