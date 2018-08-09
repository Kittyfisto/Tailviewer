using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class IntegerLiteral
		: Literal
	{
		private readonly long _value;

		public IntegerLiteral(long value)
		{
			_value = value;
		}

		#region Implementation of IExpression

		public override Type ResultType => typeof(long);

		public override object Evaluate(IReadOnlyList<LogLine> logEntry)
		{
			return _value;
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
