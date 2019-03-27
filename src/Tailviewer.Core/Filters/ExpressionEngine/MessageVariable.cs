using System;
using System.Collections.Generic;
using System.Text;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class MessageVariable
		: IExpression<string>
	{
		public const string Value = "message";

		#region Implementation of IExpression

		public Type ResultType => typeof(string);

		public string Evaluate(IReadOnlyList<LogLine> logEntry)
		{
			var builder = new StringBuilder();
			foreach(var line in logEntry)
			{
				builder.AppendLine(line.Message);
			}
			return builder.ToString();
		}

		object IExpression.Evaluate(IReadOnlyList<LogLine> logEntry)
		{
			return Evaluate(logEntry);
		}

		#endregion

		#region Overrides of Object

		public override bool Equals(object obj)
		{
			return obj is MessageVariable;
		}

		public override int GetHashCode()
		{
			return 105;
		}

		public override string ToString()
		{
			return string.Format("{0}{1}", Tokenizer.ToString(TokenType.Dollar), Value);
		}

		#endregion
	}
}
