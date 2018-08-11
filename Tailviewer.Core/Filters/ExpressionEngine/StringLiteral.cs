using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class StringLiteral
		: IExpression<string>
	{
		private readonly string _value;

		public StringLiteral(string value)
		{
			_value = value;
		}

		#region Implementation of IExpression

		public Type ResultType => typeof(string);

		public string Evaluate(IReadOnlyList<LogLine> logEntry)
		{
			return _value;
		}

		object IExpression.Evaluate(IReadOnlyList<LogLine> logEntry)
		{
			return Evaluate(logEntry);
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

		[Pure]
		public static IExpression<string> Create(IReadOnlyList<Token> tokens)
		{
			var builder = new StringBuilder();
			foreach(var token in tokens)
			{
				builder.Append(token.Value);
			}
			return new StringLiteral(builder.ToString());
		}
	}
}