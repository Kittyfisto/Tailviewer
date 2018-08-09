using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class LessThanExpression
		: IExpression<bool>
	{
		private readonly IExpression<long> _lhs;
		private readonly IExpression<long> _rhs;

		public LessThanExpression(IExpression<long> lhs, IExpression<long> rhs)
		{
			_lhs = lhs;
			_rhs = rhs;
		}

		#region Overrides of BinaryNumericExpression

		public bool Evaluate(IReadOnlyList<LogLine> logEntry)
		{
			var lhs = _lhs.Evaluate(logEntry);
			var rhs = _rhs.Evaluate(logEntry);
			return lhs < rhs;
		}

		object IExpression.Evaluate(IReadOnlyList<LogLine> logEntry)
		{
			return Evaluate(logEntry);
		}

		public Type ResultType => typeof(bool);

		#endregion

		#region Equality members

		private bool Equals(LessThanExpression other)
		{
			return _lhs.Equals(other._lhs) && _rhs.Equals(other._rhs);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is LessThanExpression && Equals((LessThanExpression) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (_lhs.GetHashCode() * 397) ^ _rhs.GetHashCode();
			}
		}

		#region Overrides of Object

		public override string ToString()
		{
			return string.Format("{0} {1} {2}", _lhs, Tokenizer.ToString(TokenType.LessThan), _rhs);
		}

		#endregion

		#endregion
	}
}
