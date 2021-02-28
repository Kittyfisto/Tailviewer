using System;
using System.Collections.Generic;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class LogLevelVariable
		: IExpression<LevelFlags>
	{
		public const string Value = "loglevel";

		#region Implementation of IExpression

		public Type ResultType => typeof(LevelFlags);

		public LevelFlags Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			using (var it = logEntry.GetEnumerator())
			{
				if (!it.MoveNext())
					return LevelFlags.None;

				return it.Current.LogLevel;
			}
		}

		object IExpression.Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			return Evaluate(logEntry);
		}

		#endregion

		#region Overrides of Object

		public override bool Equals(object obj)
		{
			return obj is LogLevelVariable;
		}

		public override int GetHashCode()
		{
			return 106;
		}

		public override string ToString()
		{
			return string.Format("{0}{1}", Tokenizer.ToString(TokenType.Dollar), Value);
		}

		#endregion
	}
}