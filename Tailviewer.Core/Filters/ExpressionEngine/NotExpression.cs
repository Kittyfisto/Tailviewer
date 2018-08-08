using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class NotExpression
		: IExpression
	{
		private readonly IExpression _expression;

		public NotExpression(IExpression expression)
		{
			_expression = expression;
		}

		public override string ToString()
		{
			return string.Format("{0}{1}", Tokenizer.ToString(TokenType.Not), _expression);
		}

		private bool Equals(NotExpression other)
		{
			return _expression.Equals(other._expression);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is NotExpression && Equals((NotExpression) obj);
		}

		public override int GetHashCode()
		{
			return _expression.GetHashCode();
		}

		public object Evaluate(IEnumerable<LogLine> logEntry)
		{
			var result = _expression.Evaluate(logEntry) as bool?;
			return !result;
		}
	}
}