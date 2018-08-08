using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class ContainsExpression
		: IExpression
	{
		private readonly IExpression _lhs;
		private readonly IExpression _rhs;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		public ContainsExpression(IExpression lhs, IExpression rhs)
		{
			_lhs = lhs;
			_rhs = rhs;
		}

		/// <inheritdoc />
		public object Evaluate(IEnumerable<LogLine> logEntry)
		{
			var lhs = _lhs.Evaluate(logEntry)?.ToString();
			var rhs = _rhs.Evaluate(logEntry)?.ToString();
			if (lhs == null)
				return false;
			if (rhs == null)
				return true;
			return lhs.Contains(rhs);
		}

		#region Overrides of Object

		public override string ToString()
		{
			return string.Format("{0} {1} {2}", _lhs, Tokenizer.ToString(TokenType.Contains), _rhs);
		}

		#endregion
	}
}