using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	/// <summary>
	/// Retrieves the line number of the log entry in question (the first line in case
	/// of a multi-line log entry).
	/// </summary>
	internal sealed class LineExpression
		: IExpression
	{
		public const string Value = "line";

		#region Implementation of IExpression

		public object Evaluate(IEnumerable<LogLine> logEntry)
		{
			using (var it = logEntry.GetEnumerator())
			{
				if (!it.MoveNext())
					return null;

				return it.Current.LineIndex;
			}
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