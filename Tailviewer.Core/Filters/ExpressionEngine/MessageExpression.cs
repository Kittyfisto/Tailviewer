using System;
using System.Collections.Generic;
using System.Text;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class MessageExpression
		: IExpression
	{
		public const string Value = "message";

		#region Implementation of IExpression

		public Type ResultType => typeof(string);

		public object Evaluate(IReadOnlyList<LogLine> logEntry)
		{
			var builder = new StringBuilder();
			foreach(var line in logEntry)
			{
				builder.AppendLine(line.Message);
			}
			return builder;
		}

		#endregion

		#region Overrides of Object

		public override string ToString()
		{
			return string.Format("{0}{1}", Tokenizer.ToString(TokenType.Dollar), Value);
		}

		#endregion
	}
}
