using System;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class LessThanExpression
		: BinaryNumericExpression
	{
		public LessThanExpression(IExpression lhs, IExpression rhs) : base(lhs, rhs)
		{}

		#region Overrides of BinaryNumericExpression

		protected override object Evaluate(long lhs, long rhs)
		{
			return lhs < rhs;
		}

		public override Type ResultType => typeof(bool);

		protected override TokenType TokenType => TokenType.LessThan;

		#endregion
	}
}
