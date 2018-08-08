using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class TrueExpression
		: IExpression
	{
		private static readonly object True = true;

		#region Implementation of IExpression

		public object Evaluate(IEnumerable<LogLine> logEntry)
		{
			return True; //< Avoids boxing
		}

		#endregion

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is TrueExpression;
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
