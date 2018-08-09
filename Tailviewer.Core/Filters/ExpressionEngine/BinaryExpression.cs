using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal abstract class BinaryExpression
		: IExpression
	{
		private readonly IExpression _lhs;
		private readonly IExpression _rhs;

		protected IExpression Lhs => _lhs;
		protected IExpression Rhs => _rhs;

		protected BinaryExpression(IExpression lhs, IExpression rhs)
		{
			_lhs = lhs;
			_rhs = rhs;
		}

		#region Implementation of IExpression

		public abstract Type ResultType { get; }

		public abstract object Evaluate(IReadOnlyList<LogLine> logEntry);

		#endregion

		protected abstract TokenType TokenType { get; }

		#region Overrides of Object

		public sealed override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			return obj.GetType() == GetType() && Equals((BinaryExpression) obj);
		}

		private bool Equals(BinaryExpression other)
		{
			return Equals(_lhs, other._lhs) &&
			       Equals(_rhs, other._rhs);
		}

		public sealed override int GetHashCode()
		{
			return 101;
		}

		public override string ToString()
		{
			return string.Format("{0} {1} {2}", Rhs, Tokenizer.ToString(TokenType), Rhs);
		}

		#endregion

		public static IExpression Create(BinaryOperation operation, IExpression lhs, IExpression rhs)
		{
			switch (operation)
			{
				case BinaryOperation.And:
					return new AndExpression((IExpression<bool>) lhs, (IExpression<bool>) rhs);

				case BinaryOperation.Or:
					return new OrExpression((IExpression<bool>) lhs, (IExpression<bool>) rhs);

				case BinaryOperation.LessThan:
					return new LessThanExpression((IExpression<long>) lhs, (IExpression<long>) rhs);

				case BinaryOperation.LessOrEquals:
					return new LessOrEqualsExpression((IExpression<long>) lhs, (IExpression<long>) rhs);

				case BinaryOperation.GreaterThan:
					return new GreaterThanExpression((IExpression<long>) lhs, (IExpression<long>) rhs);

				case BinaryOperation.GreaterOrEquals:
					return new GreaterOrEqualsExpression((IExpression<long>) lhs, (IExpression<long>) rhs);

				case BinaryOperation.Contains:
					return new ContainsStringExpression(lhs, rhs);

				case BinaryOperation.ContainsTimestamp:
					return new ContainsTimestampExpression((IExpression<DateTime?>) lhs,
					                                       (IExpression<IInterval<DateTime?>>) rhs);

				default:
					throw new NotImplementedException(string.Format("Operation not implemented: {0}", operation));
			}
		}
	}
}