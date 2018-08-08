using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class IntegerLiteral
		: IExpression
	{
		private readonly long _value;

		public IntegerLiteral(long value)
		{
			_value = value;
		}

		#region Implementation of IExpression

		public object Evaluate(IEnumerable<LogLine> logEntry)
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
