using System;
using System.Diagnostics.Contracts;

namespace Tailviewer.Core
{
	/// <summary>
	/// 
	/// </summary>
	internal static class DateTimeExtensions
	{
		/// <summary>
		/// Returns the start of the week of the given date.
		/// The start is defined as 0:0:0 on that week's monday.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Pure]
		public static DateTime StartOfWeek(this DateTime value)
		{
			int diff = (7 + (value.DayOfWeek - DayOfWeek.Monday)) % 7;
			return value.AddDays(-1 * diff).Date;
		}

		/// <summary>
		/// Returns the start of the month of the given date.
		/// The start is defined as 0:0:0 on that month's first day.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Pure]
		public static DateTime StartOfMonth(this DateTime value)
		{
			return new DateTime(value.Year,
			                    value.Month,
			                    1,
			                    0,
			                    0, 0,
			                    value.Kind);
		}

		/// <summary>
		/// Returns the start of the year of the given date.
		/// The start is defined as 0:0:0 on January 1st.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Pure]
		public static DateTime StartOfYear(this DateTime value)
		{
			return new DateTime(value.Year,
			                    1,
			                    1,
			                    0,
			                    0, 0,
			                    value.Kind);
		}
	}
}