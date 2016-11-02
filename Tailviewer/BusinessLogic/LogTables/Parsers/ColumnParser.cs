﻿using System;
using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.LogTables.Parsers
{
	public abstract class ColumnParser
	{
		public static ColumnParser Create(ColumnType type, string format)
		{
			switch (type)
			{
				case ColumnType.Logger:
					return new LoggerParser();

				case ColumnType.Level:
					return new LevelParser();

				case ColumnType.Timestamp:
					return new TimestampParser(format);

				case ColumnType.Thread:
					return new ThreadParser();

				case ColumnType.Line:
					return new LineParser();

				case ColumnType.Newline:
					return new NewlineParser();

				case ColumnType.Message:
					return new MessageParser();

				case ColumnType.Date:
					return new DateParser(format, DateTimeKind.Local);

				case ColumnType.UtcDate:
					return new DateParser(format, DateTimeKind.Utc);

				default:
					throw new KeyNotFoundException(string.Format("Unable to find a parser for '{0}'", type));
			}
		}

		public abstract object Parse(string line, int startIndex, out int numCharactersConsumed);
	}
}