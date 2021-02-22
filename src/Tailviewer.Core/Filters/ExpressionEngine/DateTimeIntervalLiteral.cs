using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Core.Settings;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class DateTimeIntervalLiteral
		: IExpression<IInterval<DateTime?>>
	{
		private static readonly TimeSpan AlmostADay = TimeSpan.FromDays(1) - TimeSpan.FromTicks(1);
		private static readonly TimeSpan AlmostAWeek = TimeSpan.FromDays(7) - TimeSpan.FromTicks(1);
		private static readonly TimeSpan AlmostAMonth = TimeSpan.FromDays(30) - TimeSpan.FromTicks(1);
		private static readonly TimeSpan AlmostAYear = TimeSpan.FromDays(365) - TimeSpan.FromTicks(1);

		private static readonly IReadOnlyDictionary<string, SpecialDateTimeInterval> SpecialValues;
		private static readonly IReadOnlyDictionary<SpecialDateTimeInterval, string> Strings;

		static DateTimeIntervalLiteral()
		{
			SpecialValues = new Dictionary<string, SpecialDateTimeInterval>
			{
				{"today", SpecialDateTimeInterval.Today}
			};
			Strings = SpecialValues.ToDictionary(pair => pair.Value, pair => pair.Key);
		}

		private readonly SpecialDateTimeInterval _interval;

		public DateTimeIntervalLiteral(SpecialDateTimeInterval interval)
		{
			_interval = interval;
		}

		#region Implementation of IExpression

		public Type ResultType => typeof(IInterval<DateTime?>);

		public IInterval<DateTime?> Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			switch (_interval)
			{
				case SpecialDateTimeInterval.Today:
					var today = DateTime.Today;
					return new Interval<DateTime?>(today, today + AlmostADay);

				case SpecialDateTimeInterval.ThisWeek:
					var startOfWeek = DateTime.Now.StartOfWeek();
					return new Interval<DateTime?>(startOfWeek, startOfWeek + AlmostAWeek);

				case SpecialDateTimeInterval.ThisMonth:
					var startOfMonth = DateTime.Now.StartOfMonth();
					return new Interval<DateTime?>(startOfMonth, startOfMonth + AlmostAMonth);

				case SpecialDateTimeInterval.ThisYear:
					var startOfYear = DateTime.Now.StartOfYear();
					return new Interval<DateTime?>(startOfYear, startOfYear + AlmostAYear);

				default: throw new NotImplementedException();
			}
		}

		object IExpression.Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
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

		public static bool TryParse(string value, out SpecialDateTimeInterval interval)
		{
			if (SpecialValues.TryGetValue(value, out interval))
				return true;

			return false;
		}
	}
}
