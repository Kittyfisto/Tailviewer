using System.Collections.Generic;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Core.LogTables.Parsers
{
	/// <summary>
	/// Responsible for parsing a log4net level from 
	/// </summary>
	public sealed class LevelParser
		: ColumnParser
	{
		private readonly Dictionary<string, LevelFlags> _levels;

		/// <summary>
		/// Initializes this parser.
		/// </summary>
		public LevelParser()
		{
			_levels = new Dictionary<string, LevelFlags>
				{
					{"DEBUG", LevelFlags.Debug},
					{"INFO", LevelFlags.Info},
					{"WARNING", LevelFlags.Warning},
					{"ERROR", LevelFlags.Error},
					{"FATAL", LevelFlags.Fatal},
				};
		}

		/// <inheritdoc />
		public override object Parse(string line, int startIndex, out int numCharactersConsumed)
		{
			foreach (var pair in _levels)
			{
				var value = line.Substring(startIndex);
				if (value.StartsWith(pair.Key))
				{
					numCharactersConsumed = pair.Key.Length;
					return pair.Value;
				}
			}

			numCharactersConsumed = 0;
			return null;
		}
	}
}