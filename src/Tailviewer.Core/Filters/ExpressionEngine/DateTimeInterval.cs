using System;
using System.Collections.Generic;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class DateTimeInterval
		: IExpression<IInterval<DateTime?>>
	{
		private readonly DateTime? _minimum;
		private readonly DateTime? _maximum;

		public DateTimeInterval(DateTime? minimum, DateTime? maximum)
		{
			_minimum = minimum;
			_maximum = maximum;
		}

		#region Overrides of Object

		public override string ToString()
		{
			return $"({_minimum}, {_maximum})";
		}

		#endregion

		#region Implementation of IExpression

		public Type ResultType => typeof(IInterval<DateTime?>);

		public IInterval<DateTime?> Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			return new Interval<DateTime?>(_minimum, _maximum);
		}

		object IExpression.Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			return Evaluate(logEntry);
		}

		#endregion
	}
}