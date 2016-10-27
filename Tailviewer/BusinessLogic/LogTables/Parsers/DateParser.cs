using System;
using System.Globalization;

namespace Tailviewer.BusinessLogic.LogTables.Parsers
{
	public sealed class DateParser
		: ColumnParser
	{
		private readonly string _format;
		private readonly DateTimeStyles _style;

		public DateParser(string format, DateTimeKind kind)
		{
			switch (kind)
			{
				case DateTimeKind.Local:
					_style = DateTimeStyles.AssumeLocal;
					break;

				case DateTimeKind.Utc:
					_style = DateTimeStyles.AssumeUniversal;
					break;

				default:
					throw new ArgumentOutOfRangeException("kind");
			}

			switch (format)
			{
				case "ABSOLUTE":
					break;

				case "ISO8601":
					break;

				case "DATE":
					break;
			}
		}

		public override object Parse(string line, int startIndex, out int numCharactersConsumed)
		{
			DateTime dateTime;
			if (!DateTime.TryParseExact(line, _format, CultureInfo.InvariantCulture, _style, out dateTime))
			{
				numCharactersConsumed = 0;
				return null;
			}

			numCharactersConsumed = -1;
			return dateTime;
		}
	}
}