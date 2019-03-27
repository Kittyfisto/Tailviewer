using System;

namespace Tailviewer.Core.LogTables.Parsers
{
	/// <summary>
	///     Not implemented yet.
	/// </summary>
	public sealed class TimestampParser
		: ColumnParser
	{
		private readonly string _format;

		/// <summary>
		///     Initializes this parser with the given format.
		/// </summary>
		/// <param name="format"></param>
		public TimestampParser(string format)
		{
			_format = format;
		}

		/// <inheritdoc />
		public override object Parse(string line, int startIndex, out int numCharactersConsumed)
		{
			throw new NotImplementedException();
		}
	}
}