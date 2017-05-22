using System;
using System.Globalization;

namespace Tailviewer.BusinessLogic.LogFiles.Parsers
{
	public sealed class TimeOfDaySecondsSinceStartParser
		: ITimestampParser
	{
		public bool TryParse(string content, out DateTime timestamp)
		{
			timestamp = DateTime.MinValue;

			if (content == null)
				return false;

			var timeOfDayIndex = content.IndexOf(";", 0, StringComparison.CurrentCulture);
			if (timeOfDayIndex == -1)
				return false;

			var secondsSinceStartIndex = content.IndexOf(";", timeOfDayIndex+1, StringComparison.CurrentCulture);
			if (secondsSinceStartIndex == -1)
				return false;

			int hours, minutes, seconds;
			if (!TryParseTimeOfDay(content, 0, timeOfDayIndex, out hours, out minutes, out seconds))
				return false;

			float secondsSinceStart;
			if (!TryParseSecondsSinceStart(content, timeOfDayIndex + 1, secondsSinceStartIndex - timeOfDayIndex - 1,
				out secondsSinceStart))
				return false;

			var miliseconds = (int) ((secondsSinceStart - (int) secondsSinceStart) * 1000);
			var today = DateTime.Today;
			timestamp = new DateTime(today.Year,
				today.Month,
				today.Day,
				hours, minutes, seconds, miliseconds,
				DateTimeKind.Unspecified);
			return true;
		}

		private bool TryParseTimeOfDay(string content,
			int startIndex, int length,
			out int hours,
			out int minutes,
			out int seconds)
		{
			hours = 0;
			minutes = 0;
			seconds = 0;

			var minutesIndex = content.IndexOf(":", startIndex, StringComparison.CurrentCulture);
			if (minutesIndex == -1)
				return false;

			var secondsIndex = content.IndexOf(":", minutesIndex + 1, StringComparison.CurrentCulture);
			if (secondsIndex == -1)
				return false;

			if (secondsIndex + 1 >= content.Length)
				return false;

			var hoursValue = content.Substring(startIndex, minutesIndex-startIndex);
			var minutesValue = content.Substring(minutesIndex + 1, secondsIndex - minutesIndex - 1);
			var secondsValue = content.Substring(secondsIndex + 1, length - secondsIndex - 1);

			if (!int.TryParse(hoursValue, out hours) ||
			    !int.TryParse(minutesValue, out minutes) ||
			    !int.TryParse(secondsValue, out seconds))
				return false;

			return true;
		}

		private bool TryParseSecondsSinceStart(string content, int timeOfDayIndex, int length, out float secondsSinceStart)
		{
			var value = content.Substring(timeOfDayIndex, length).Trim();
			if (!float.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out secondsSinceStart))
				return false;

			return true;
		}
	}
}