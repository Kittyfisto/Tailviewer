using System;
using System.Collections.Generic;
using Tailviewer.Api;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class ContainsTimestampExpression
		: IExpression<bool>
	{
		private readonly IExpression<IInterval<DateTime?>> _lhs;
		private readonly IExpression<DateTime?> _rhs;

		public ContainsTimestampExpression(IExpression<IInterval<DateTime?>> lhs, IExpression<DateTime?> rhs)
		{
			_lhs = lhs;
			_rhs = rhs;
		}

		public static IExpression Create(IExpression<IInterval<DateTime?>> lhs, IExpression<DateTime?> rhs)
		{
			return new ContainsTimestampExpression(lhs, rhs);
		}

		#region Overrides of BinaryExpression

		public Type ResultType => typeof(bool);

		public bool Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			var lhs = _lhs.Evaluate(logEntry);
			var rhs = _rhs.Evaluate(logEntry);

			if (lhs == null)
				return false;

			if (rhs == null)
				return false;

			var minimum = lhs.Minimum;
			var maximum = lhs.Maximum;

			if (minimum == null && maximum == null)
				return false;

			if (minimum != null)
			{
				if (rhs < minimum)
					return false;
			}

			if (maximum != null)
			{
				if (rhs > maximum)
					return false;
			}

			return true;
		}

		object IExpression.Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
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
			return string.Format("{0} {1} {2}", _lhs, Tokenizer.ToString(TokenType.Contains), _rhs);
		}

		#endregion

		#endregion
	}
}