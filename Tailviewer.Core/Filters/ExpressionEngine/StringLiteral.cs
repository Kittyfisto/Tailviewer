using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class StringLiteral
		: IExpression
	{
		private readonly string _value;

		public StringLiteral(string value)
		{
			_value = value;
		}

		#region Implementation of IExpression

		public Type ResultType => typeof(string);

		public object Evaluate(IReadOnlyList<LogLine> logEntry)
		{
			return _value;
		}

		#endregion

		#region Overrides of Object

		public override bool Equals(object obj)
		{
			return obj is StringLiteral && Equals((StringLiteral) obj);
		}

		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		private bool Equals(StringLiteral other)
		{
			return _value == other._value;
		}

		public override string ToString()
		{
			return string.Format("\"{0}\"", _value);
		}

		#endregion
	}
}