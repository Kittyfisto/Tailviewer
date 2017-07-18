namespace Tailviewer.Core.LogTables.Parsers
{
	public sealed class TimestampParser
		: ColumnParser
	{
		private readonly string _format;

		public TimestampParser(string format)
		{
			_format = format;
		}

		public override object Parse(string line, int startIndex, out int numCharactersConsumed)
		{
			throw new System.NotImplementedException();
		}
	}
}