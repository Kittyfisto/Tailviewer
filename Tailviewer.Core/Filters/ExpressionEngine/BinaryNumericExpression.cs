using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal abstract class BinaryNumericExpression
		: BinaryExpression
	{
		protected BinaryNumericExpression(IExpression lhs, IExpression rhs)
			: base(lhs, rhs)
		{}

		#region Implementation of IExpression

		public override object Evaluate(IReadOnlyList<LogLine> logEntry)
		{
			var lhs = Lhs.Evaluate(logEntry);
			var rhs = Rhs.Evaluate(logEntry);
			var lhsValue = Convert.ToInt64(lhs);
			var rhsValue = Convert.ToInt64(rhs);
			return Evaluate(lhsValue, rhsValue);
		}

		protected abstract object Evaluate(long lhs, long rhs);

		#endregion
	}
}