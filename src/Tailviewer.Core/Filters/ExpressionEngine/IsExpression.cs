using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Tailviewer.Api;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class IsExpression<T>
		: IExpression<bool>
	{
		private static readonly IEqualityComparer<T> Comparer;
		private readonly IExpression<T> _lhs;
		private readonly IExpression<T> _rhs;

		static IsExpression()
		{
			// The default comparer sucks for custom value types as it will cause boxing on every invocation
			// (https://www.jacksondunstan.com/articles/5148), so we will use our own comparers for types known
			// to us.
			if (typeof(T) == typeof(LevelFlags))
			{
				Comparer = (IEqualityComparer<T>)(object)new LevelFlagsComparer();
			}
			else
			{
				Comparer = EqualityComparer<T>.Default;
			}
		}

		public IsExpression(IExpression<T> lhs, IExpression<T> rhs)
		{
			_lhs = lhs;
			_rhs = rhs;
		}

		#region Implementation of IExpression

		public Type ResultType => typeof(bool);

		public bool Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			var lhs = _lhs.Evaluate(logEntry);
			var rhs = _rhs.Evaluate(logEntry);

			return Comparer.Equals(lhs, rhs);
		}

		object IExpression.Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			return Evaluate(logEntry);
		}

		#endregion

		#region Equality members

		private bool Equals(IsExpression<T> other)
		{
			return Equals(_lhs, other._lhs) && Equals(_rhs, other._rhs);
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is IsExpression<T> other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((_lhs != null ? _lhs.GetHashCode() : 0) * 397) ^ (_rhs != null ? _rhs.GetHashCode() : 0);
			}
		}

		public static bool operator ==(IsExpression<T> left, IsExpression<T> right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(IsExpression<T> left, IsExpression<T> right)
		{
			return !Equals(left, right);
		}

		#endregion

		#region Overrides of Object

		public override string ToString()
		{
			return string.Format("{0} {1} {2}", _lhs, Tokenizer.ToString(TokenType.Is), _rhs);
		}

		#endregion

		[Pure]
		public static IsExpression<T> Create(IExpression<T> lhs, IExpression<T> rhs)
		{
			return new IsExpression<T>(lhs, rhs);
		}
	}
}