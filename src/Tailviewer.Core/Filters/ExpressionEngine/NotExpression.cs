using System;
using System.Collections.Generic;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class NotExpression
		: IExpression<bool>
	{
		private readonly IExpression<bool> _expression;

		public NotExpression(IExpression<bool> expression)
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

		public Type ResultType => typeof(bool);

		public bool Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			var result = _expression.Evaluate(logEntry);
			return !result;
		}

		object IExpression.Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			return Evaluate(logEntry);
		}
	}
}