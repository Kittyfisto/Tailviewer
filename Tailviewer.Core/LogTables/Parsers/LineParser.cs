using System;

namespace Tailviewer.Core.LogTables.Parsers
{
	public sealed class LineParser
		: ColumnParser
	{
		public override object Parse(string line, int startIndex, out int numCharactersConsumed)
		{
			throw new NotImplementedException();
		}
	}
}