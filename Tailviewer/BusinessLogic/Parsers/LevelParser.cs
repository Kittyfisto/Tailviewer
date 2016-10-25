using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.Parsers
{
	public sealed class LevelParser
		: ColumnParser
	{
		private readonly Dictionary<string, LevelFlags> _levels;

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