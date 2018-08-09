using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class DateTimeIntervalLiteral
		: IExpression<IInterval<DateTime?>>
	{
		private static readonly TimeSpan AlmostADay = TimeSpan.FromDays(1) - TimeSpan.FromTicks(1);
		private static readonly IReadOnlyDictionary<string, DateTimeInterval> SpecialValues;
		private static readonly IReadOnlyDictionary<DateTimeInterval, string> Strings;

		static DateTimeIntervalLiteral()
		{
			SpecialValues = new Dictionary<string, DateTimeInterval>
			{
				{"today", DateTimeInterval.Today}
			};
			Strings = SpecialValues.ToDictionary(pair => pair.Value, pair => pair.Key);
		}

		private readonly DateTimeInterval _interval;

		public DateTimeIntervalLiteral(DateTimeInterval interval)
		{
			_interval = interval;
		}

		#region Implementation of IExpression

		public Type ResultType => typeof(IInterval<DateTime?>);

		public IInterval<DateTime?> Evaluate(IReadOnlyList<LogLine> logEntry)
		{
			var today = DateTime.Today;
			switch (_interval)
			{
				case DateTimeInterval.Today:
					return new Interval<DateTime?>(today, today + AlmostADay);

				default: throw new NotImplementedException();
			}
		}

		object IExpression.Evaluate(IReadOnlyList<LogLine> logEntry)
		{
			return Evaluate(logEntry);
		}

		#endregion

		#region Overrides of Object

		#region Equality members

		private bool Equals(DateTimeIntervalLiteral other)
		{
			return _interval == other._interval;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is DateTimeIntervalLiteral && Equals((DateTimeIntervalLiteral) obj);
		}

		public override int GetHashCode()
		{
			return (int) _interval;
		}

		public override string ToString()
		{
			Strings.TryGetValue(_interval, out var str);
			return str ?? string.Empty;
		}

		#endregion

		#endregion

		public static bool TryParse(string value, out DateTimeInterval interval)
		{
			if (SpecialValues.TryGetValue(value, out interval))
				return true;

			return false;
		}
	}
}
