using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class FalseExpression
		: IExpression
	{
		private static readonly object False = false;

		#region Implementation of IExpression

		public object Evaluate(IEnumerable<LogLine> logEntry)
		{
			return False; //< Avoids boxing
		}

		#endregion

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is FalseExpression;
		}

		public override int GetHashCode()
		{
			return 42;
		}

		#region Overrides of Object

		public override string ToString()
		{
			return Tokenizer.ToString(TokenType.False);
		}

		#endregion
	}
}