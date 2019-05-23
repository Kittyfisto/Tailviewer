using System;
using System.Globalization;
using System.Windows.Navigation;

namespace Tailviewer.Core.Parsers
{
	/// <summary>
	///     A parser responsible for parsing strings which represent a point in time into
	///     <see cref="DateTime" /> values.
	/// </summary>
	public sealed class DateTimeParser
		: ITimestampParser
	{
		private readonly string _format;

		/// <summary>
		///     Intitializes this parser with the given format,
		///     <see
		///         cref="DateTime.TryParseExact(string,string,System.IFormatProvider,System.Globalization.DateTimeStyles,out System.DateTime)" />
		///     for possible format strings to use.
		/// </summary>
		/// <param name="format"></param>
		public DateTimeParser(string format)
		{
			_format = format;
		}

		/// <inheritdoc />
		public int MinimumLength => _format.Length;

		/// <inheritdoc />
		public override string ToString()
		{
			return _format;
		}

		/// <inheritdoc />
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