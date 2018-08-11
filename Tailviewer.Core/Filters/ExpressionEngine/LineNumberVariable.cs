using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	/// <summary>
	/// Retrieves the line number of the log entry in question (the first line in case
	/// of a multi-line log entry).
	/// </summary>
	internal sealed class LineNumberVariable
		: IExpression<long>
	{
		public const string Value = "linenumber";

		#region Implementation of IExpression

		public Type ResultType => typeof(long);

		public long Evaluate(IReadOnlyList<LogLine> logEntry)
		{
			return logEntry[0].LineIndex + 1;
		}

		object IExpression.Evaluate(IReadOnlyList<LogLine> logEntry)
		{
			return Evaluate(logEntry);
		}

		#endregion

		#region Overrides of Object

		public override bool Equals(object obj)
		{
			return obj is LineNumberVariable;
		}

		public override int GetHashCode()
		{
			return 104;
		}

		public override string ToString()
		{
			return string.Format("{0}{1}", Tokenizer.ToString(TokenType.Dollar), Value);
		}

		#endregion
	}
}