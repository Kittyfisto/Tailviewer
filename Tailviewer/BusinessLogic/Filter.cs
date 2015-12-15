using System.Collections.Generic;
using System.Linq;

namespace Tailviewer.BusinessLogic
{
	internal static class Filter
	{
		public static IFilter Create(IEnumerable<IFilter> filters)
		{
			List<IFilter> tmp = filters.ToList();
			if (tmp.Count == 0)
				return null;

			if (tmp.Count == 1)
				return tmp[0];
			return new FilterChain(tmp);
		}

		public static IFilter Create(string substringFilter)
		{
			var filter = new SubstringFilter(substringFilter, true);
			return filter;
		}

		public static IFilter Create(string substringFilter,
		                             LevelFlags levelFilter,
		                             bool excludeOther)
		{
			return Create(CreateFilters(substringFilter, true, levelFilter, excludeOther));
		}

		public static List<IFilter> CreateFilters(string substringFilter,
		                                          bool ignoreCase,
		                                          LevelFlags levelFilter,
		                                          bool excludeOther)
		{
			var filters = new List<IFilter>();
			if (!string.IsNullOrEmpty(substringFilter))
				filters.Add(new SubstringFilter(substringFilter, ignoreCase));
			if (levelFilter != LevelFlags.All || excludeOther)
				filters.Add(new LevelFilter(levelFilter, excludeOther));
			return filters;
		}

		public static IFilter Create(string substringFilter,
		                             bool ignoreCase,
		                             LevelFlags levelFilter,
		                             bool excludeOther,
		                             IEnumerable<IFilter> additionalFilters = null)
		{
			List<IFilter> filters = CreateFilters(substringFilter, ignoreCase, levelFilter, excludeOther);
			if (additionalFilters != null)
				filters.AddRange(additionalFilters);
			return Create(filters);
		}
	}
}