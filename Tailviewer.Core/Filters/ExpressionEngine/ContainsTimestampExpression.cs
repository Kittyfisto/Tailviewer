using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class ContainsTimestampExpression
		: IExpression<bool>
	{
		private readonly IExpression<DateTime?> _lhs;
		private readonly IExpression<IInterval<DateTime?>> _rhs;

		public ContainsTimestampExpression(IExpression<DateTime?> lhs, IExpression<IInterval<DateTime?>> rhs)
		{
			_lhs = lhs;
			_rhs = rhs;
		}

		#region Overrides of BinaryExpression

		public Type ResultType => typeof(bool);

		public bool Evaluate(IReadOnlyList<LogLine> logEntry)
		{
			var lhs = _lhs.Evaluate(logEntry);
			var rhs = _rhs.Evaluate(logEntry);

			if (rhs == null)
				return false;

			if (lhs == null)
				return false;

			return rhs.Minimum >= lhs &&
			       rhs.Maximum <= lhs;
		}

		object IExpression.Evaluate(IReadOnlyList<LogLine> logEntry)
		{
			return Evaluate(logEntry);
		}

		#endregion

		#region Equality members

		private bool Equals(ContainsTimestampExpression other)
		{
			return _lhs.Equals(other._lhs) && _rhs.Equals(other._rhs);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is ContainsTimestampExpression && Equals((ContainsTimestampExpression) obj);
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
			return string.Format("{0} {1} {2}", _lhs, Tokenizer.ToString(TokenType.Is), _rhs);
		}

		#endregion

		#endregion
	}
}