using System;
using System.Globalization;

namespace Tailviewer.AcceptanceTests.BusinessLogic.Sources
{
	internal sealed class CustomTimestampParser : ITimestampParser
	{
		public int MinimumLength => 1;

		public bool TryParse(string content, out DateTime timestamp)
		{
			if (TryParse24CharacterTimestamp(content, out timestamp))
				return true;

			if (TryParse23CharacterTimestamp(content, out timestamp))
				return true;

			return false;
		}

		private bool TryParse23CharacterTimestamp(string content, out DateTime timestamp)
		{
			// Example strings:
			// "2019-03-18 14:09:54:177"
			// "29/03/2019 14:09:54:177"

			const int timestampPartLength = 23;
			var formats = new[]
			{
				"yyyy-MM-dd HH:mm:ss:fff",
				"dd/MM/yyyy HH:mm:ss:fff"
			};
			return TryParseExact(content, timestampPartLength, formats, out timestamp);
		}

		private bool TryParse24CharacterTimestamp(string content, out DateTime timestamp)
		{
			// Example strings:
			// "2019-03-18  14:09:54:177"
			// "29/03/2019  14:09:54:177"

			const int timestampPartLength = 24;
			var formats = new[]
			{
				"yyyy-MM-dd  HH:mm:ss:fff",
				"dd/MM/yyyy  HH:mm:ss:fff"
			};
			return TryParseExact(content, timestampPartLength, formats, out timestamp);
		}

		private static bool TryParseExact(string content, int timestampPartLength, string[] formats,
			out DateTime timestamp)
		{
			if (content.Length < timestampPartLength)
			{
				timestamp = DateTime.MinValue;
				return false;
			}

			var timestampPart = content.Substring(0, timestampPartLength);
			return DateTime.TryParseExact(timestampPart, formats, CultureInfo.InvariantCulture,
				DateTimeStyles.AssumeLocal,
				out timestamp);
		}
	}
}