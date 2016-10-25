using System;

namespace Tailviewer.BusinessLogic.LogTables.Parsers
{
	public sealed class DateParser
		: ColumnParser
	{
		private readonly string _format;

		public DateParser(string format)
		{
			_format = format;
		}

		public override object Parse(string line, int startIndex, out int numCharactersConsumed)
		{
			throw new NotImplementedException();
		}
	}
}