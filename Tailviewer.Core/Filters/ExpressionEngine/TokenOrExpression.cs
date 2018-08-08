namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal struct TokenOrExpression
	{
		public readonly Token Token;
		public readonly IExpression Expression;

		public TokenOrExpression(Token token)
		{
			Token = token;
			Expression = null;
		}

		public override string ToString()
		{
			if (Expression != null)
			{
				return Expression.ToString();
			}

			return Token.ToString();
		}

		public static implicit operator TokenOrExpression(Token token)
		{
			return new TokenOrExpression(token);
		}

		public TokenOrExpression(IExpression expression)
		{
			Token = default(Token);
			Expression = expression;
		}
	}
}