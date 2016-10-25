using System;

namespace Tailviewer.BusinessLogic.Parsers
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