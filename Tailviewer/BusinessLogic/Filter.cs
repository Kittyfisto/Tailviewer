using System;
using System.Collections.Generic;

namespace Tailviewer.BusinessLogic
{
	internal static class Filter
	{
		public static IFilter CreateFilter(string substringFilter, StringComparison comparison, LevelFlags levelFilter, bool excludeOther)
		{
			var filters = new List<IFilter>();
			if (!string.IsNullOrEmpty(substringFilter))
				filters.Add(new SubstringFilter(substringFilter, comparison));
			if (levelFilter != LevelFlags.All || excludeOther)
				filters.Add(new LevelFilter(levelFilter, excludeOther));

			if (filters.Count == 0)
				return null;

			if (filters.Count == 1)
				return filters[0];

			return new FilterChain(filters);
		}
	}
}