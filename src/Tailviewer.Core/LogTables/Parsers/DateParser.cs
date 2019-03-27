using System;
using System.Globalization;

namespace Tailviewer.Core.LogTables.Parsers
{
	/// <summary>
	///     A parser responsible for parsing a string representing a point in time into <see cref="DateTime" /> values.
	/// </summary>
	public sealed class DateParser
		: ColumnParser
	{
		private readonly string _format;
		private readonly DateTimeStyles _style;

		/// <summary>
		///     Initializes this parser.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="kind"></param>
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
					throw new ArgumentOutOfRangeException(nameof(kind));
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
			_format = format;
		}

		/// <inheritdoc />
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