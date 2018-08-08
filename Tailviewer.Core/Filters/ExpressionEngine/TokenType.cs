namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal enum TokenType
	{
		Invalid = 0,

		Whitespace,

		OpenBracket,
		CloseBracket,

		#region Binary Operators
		Equals,
		NotEquals,
		LessThan,
		LessOrEquals,
		GreaterThan,
		GreaterOrEquals,
		And,
		Or,
		Contains,
		#endregion

		#region Unary Operators
		Not,
		#endregion

		Quotation,
		Dollar,
		Literal,
		True,
		False
	}
}