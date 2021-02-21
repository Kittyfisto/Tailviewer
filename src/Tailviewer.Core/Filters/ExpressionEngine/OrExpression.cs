using System;
using System.Collections.Generic;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class OrExpression
		: IExpression<bool>
	{
		private readonly IExpression<bool> _lhs;
		private readonly IExpression<bool> _rhs;

		public OrExpression(IExpression<bool> lhs, IExpression<bool> rhs)
		{
			_lhs = lhs;
			_rhs = rhs;
		}

		public static OrExpression Create(IExpression<bool> lhs, IExpression<bool> rhs)
		{
			return new OrExpression(lhs, rhs);
		}

		#region Overrides of BinaryExpression

		public Type ResultType => typeof(bool);

		public bool Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			var lhs = _lhs.Evaluate(logEntry);
			var rhs = _rhs.Evaluate(logEntry);
			return lhs | rhs;
		}

		object IExpression.Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			return Evaluate(logEntry);
		}

		#endregion

		#region Equality members

		private bool Equals(OrExpression other)
		{
			return _lhs.Equals(other._lhs) && _rhs.Equals(other._rhs);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is OrExpression && Equals((OrExpression) obj);
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
			return string.Format("{0} {1} {2}", _lhs, Tokenizer.ToString(TokenType.Or), _rhs);
		}

		#endregion

		#endregion
	}
}