using System;
using System.Globalization;

namespace Tailviewer.BusinessLogic.LogFiles.Parsers
{
	public sealed class DateTimeParser
		: ITimestampParser
	{
		private readonly string _format;

		public DateTimeParser(string format)
		{
			_format = format;
		}

		public bool TryParse(string content, out DateTime timestamp)
		{
			return DateTime.TryParseExact(content,
				_format,
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out timestamp);
		}
	}
}