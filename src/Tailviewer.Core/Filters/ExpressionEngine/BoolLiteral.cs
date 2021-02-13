using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class BoolLiteral
		: IExpression<bool>
	{
		private readonly bool _value;

		public BoolLiteral(bool value)
		{
			_value = value;
		}

		#region Implementation of IExpression

		public Type ResultType => typeof(bool);

		public bool Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			return _value;
		}

		object IExpression.Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			return Evaluate(logEntry);
		}

		#endregion

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is BoolLiteral;
		}

		public override int GetHashCode()
		{
			return 42;
		}

		#region Overrides of Object

		public override string ToString()
		{
			return Tokenizer.ToString(TokenType.True);
		}

		#endregion
	}
}
