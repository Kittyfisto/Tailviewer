using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class GreaterOrEqualsExpression
		: IExpression<bool>
	{
		private readonly IExpression<long> _lhs;
		private readonly IExpression<long> _rhs;

		public GreaterOrEqualsExpression(IExpression<long> lhs, IExpression<long> rhs)
		{
			_lhs = lhs;
			_rhs = rhs;
		}

		public static IExpression Create(IExpression<long> lhs, IExpression<long> rhs)
		{
			return new GreaterOrEqualsExpression(lhs, rhs);
		}

		#region Implementation of IExpression

		public Type ResultType => typeof(long);

		public bool Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			var lhs = _lhs.Evaluate(logEntry);
			var rhs = _rhs.Evaluate(logEntry);
			return lhs >= rhs;
		}

		object IExpression.Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			return Evaluate(logEntry);
		}

		#endregion

		#region Equality members

		private bool Equals(GreaterOrEqualsExpression other)
		{
			return _lhs.Equals(other._lhs) && _rhs.Equals(other._rhs);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is GreaterOrEqualsExpression && Equals((GreaterOrEqualsExpression) obj);
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
			return string.Format("{0} {1} {2}", _lhs, Tokenizer.ToString(TokenType.GreaterOrEquals), _rhs);
		}

		#endregion

		#endregion
	}
}