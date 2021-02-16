using System;
using System.Collections.Generic;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class IntegerLiteral
		: IExpression<long>
	{
		private readonly long _value;

		public IntegerLiteral(long value)
		{
			_value = value;
		}

		#region Implementation of IExpression

		public Type ResultType => typeof(long);

		public long Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			return _value;
		}

		object IExpression.Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			return Evaluate(logEntry);
		}

		#endregion

		#region Overrides of Object

		public override bool Equals(object obj)
		{
			return obj is IntegerLiteral && Equals((IntegerLiteral) obj);
		}

		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		private bool Equals(IntegerLiteral other)
		{
			return _value == other._value;
		}

		public override string ToString()
		{
			return _value.ToString();
		}

		#endregion
	}
}
