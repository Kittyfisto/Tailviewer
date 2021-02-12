using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class DateTimeLiteral
		: IExpression<DateTime?>
	{
		private readonly DateTime? _value;

		public DateTimeLiteral(DateTime? value)
		{
			_value = value;
		}

		#region Implementation of IExpression

		public Type ResultType => typeof(DateTime?);

		public DateTime? Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			return _value;
		}

		object IExpression.Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			return Evaluate(logEntry);
		}

		#endregion
	}
}