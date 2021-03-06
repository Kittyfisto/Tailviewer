﻿using System;
using System.Collections.Generic;
using Tailviewer.Api;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class AndExpression
		: IExpression<bool>
	{
		private readonly IExpression<bool> _lhs;
		private readonly IExpression<bool> _rhs;

		public AndExpression(IExpression<bool> lhs, IExpression<bool> rhs)
		{
			_lhs = lhs;
			_rhs = rhs;
		}

		public static IExpression Create(IExpression<bool> lhs, IExpression<bool> rhs)
		{
			return new AndExpression(lhs, rhs);
		}

		#region Overrides of BinaryExpression

		public Type ResultType => typeof(bool);

		object IExpression.Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			return Evaluate(logEntry);
		}

		public bool Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			var lhs = _lhs.Evaluate(logEntry);
			var rhs = _rhs.Evaluate(logEntry);
			return lhs & rhs;
		}

		#region Equality members

		private bool Equals(AndExpression other)
		{
			return _lhs.Equals(other._lhs) && _rhs.Equals(other._rhs);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is AndExpression && Equals((AndExpression) obj);
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
			return string.Format("{0} {1} {2}", _lhs, Tokenizer.ToString(TokenType.And), _rhs);
		}

		#endregion

		#endregion

		#endregion
	}
}