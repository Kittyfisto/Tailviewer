using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.LogTables
{
	public abstract class ColumnParser
	{
		public abstract ColumnType Type { get; }

		public static ColumnParser Create(string pattern, ColumnType type)
		{
			switch (type)
			{
				case ColumnType.Logger:
					return new LoggerParser();

				default:
					throw new KeyNotFoundException(string.Format("Unable to find a parser for '{0}'", type));
			}
		}

		public abstract object Parse(string line, int startIndex, out int numCharactersConsumed);
	}
}