using System;
using System.Collections.Generic;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal static class BinaryExpression
	{
		private static readonly IReadOnlyDictionary<BinaryOperation, Func<IExpression, IExpression, IExpression>> Operations;

		static BinaryExpression()
		{
			Operations = new Dictionary<BinaryOperation, Func<IExpression, IExpression, IExpression>>
			{
				{BinaryOperation.And, (lhs, rhs) => Create2<bool, bool>(BinaryOperation.And, lhs, rhs, AndExpression.Create)},
				{BinaryOperation.Or, (lhs, rhs) => Create2<bool, bool>(BinaryOperation.Or, lhs, rhs, OrExpression.Create)},
				{BinaryOperation.LessThan, (lhs, rhs) => Create2<long, long>(BinaryOperation.LessThan, lhs, rhs, LessThanExpression.Create)},
				{BinaryOperation.LessOrEquals, (lhs, rhs) => Create2<long, long>(BinaryOperation.LessOrEquals, lhs, rhs, LessOrEqualsExpression.Create)},
				{BinaryOperation.GreaterThan, (lhs, rhs) => Create2<long, long>(BinaryOperation.GreaterThan, lhs, rhs, GreaterThanExpression.Create)},
				{BinaryOperation.GreaterOrEquals, (lhs, rhs) => Create2<long, long>(BinaryOperation.GreaterOrEquals, lhs, rhs, GreaterOrEqualsExpression.Create)},
				{BinaryOperation.Contains, (lhs, rhs) => Create2<string, string>(BinaryOperation.Contains, lhs, rhs, ContainsStringExpression.Create)},
				{BinaryOperation.ContainsTimestamp, (lhs, rhs) => Create2<DateTime?, IInterval<DateTime?>>(BinaryOperation.ContainsTimestamp, lhs, rhs, ContainsTimestampExpression.Create)},
			};
		}

		private static IExpression Create2<TLhs, TRhs>(BinaryOperation operation,
		                                               IExpression lhs, IExpression rhs,
		                                               Func<IExpression<TLhs>, IExpression<TRhs>, IExpression> ctor)
		{
			if (!(lhs is IExpression<TLhs>))
			{
				var expression = PrintExpression(lhs, operation, rhs);
				var message = BuildTypeErrorMessage(expression, "left hand side", typeof(TLhs));
				throw new ParseException(message);
			}

			if (!(rhs is IExpression<TRhs>))
			{
				var expression = PrintExpression(lhs, operation, rhs);
				var message = BuildTypeErrorMessage(expression, "right hand side", typeof(TRhs));
				throw new ParseException(message);
			}

			return ctor((IExpression<TLhs>) lhs, (IExpression<TRhs>) rhs);
		}

		private static string PrintExpression(IExpression lhs, BinaryOperation operation, IExpression rhs)
		{
			ExpressionParser.BinaryOperations.Backward.TryGetValue(operation, out var token);
			return string.Format("{0} {1} {2}", lhs, Tokenizer.ToString(token), rhs);
		}

		private static string BuildTypeErrorMessage(string expression, string side, Type expectedType)
		{
			return string.Format("Expected {0} of '{1}' to evaluate to a {2}", side, expression, GetTypeName(expectedType));
		}

		private static string GetTypeName(Type type)
		{
			if (type == typeof(long))
				return "number";

			if (type == typeof(bool))
				return "boolean";

			throw new NotImplementedException();
		}

		public static IExpression Create(BinaryOperation operation, IExpression lhs, IExpression rhs)
		{
			if (!Operations.TryGetValue(operation, out var factory))
				throw new NotImplementedException(string.Format("Operation not implemented: {0}", operation));

			return factory(lhs, rhs);
		}
	}
}