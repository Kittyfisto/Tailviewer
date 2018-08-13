using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class DateTimeInterval
		: IExpression<IInterval<DateTime?>>
	{
		private readonly DateTime? _start;
		private readonly DateTime? _end;

		public DateTimeInterval(DateTime? start, DateTime? end)
		{
			_start = start;
			_end = end;
		}

		#region Implementation of IExpression

		public Type ResultType => typeof(IInterval<DateTime?>);

		public IInterval<DateTime?> Evaluate(IReadOnlyList<LogLine> logEntry)
		{
			return new Interval<DateTime?>(_start, _end);
		}

		object IExpression.Evaluate(IReadOnlyList<LogLine> logEntry)
		{
			return Evaluate(logEntry);
		}

		#endregion
	}
}