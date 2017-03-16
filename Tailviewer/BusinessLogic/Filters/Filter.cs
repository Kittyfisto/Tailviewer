using System.Collections.Generic;
using System.Linq;

namespace Tailviewer.BusinessLogic.Filters
{
	public static class Filter
	{
		public static ILogEntryFilter Create(IEnumerable<ILogEntryFilter> filters)
		{
			List<ILogEntryFilter> tmp = filters.ToList();
			if (tmp.Count == 0)
				return null;

			if (tmp.Count == 1)
				return tmp[0];
			return new FilterChain(tmp);
		}

		public static ILogEntryFilter Create(string substringFilter)
		{
			var filter = new SubstringFilter(substringFilter, true);
			return filter;
		}

		public static ILogEntryFilter Create(string substringFilter,
		                                     LevelFlags levelFilter)
		{
			return Create(CreateFilters(substringFilter, true, levelFilter));
		}

		public static List<ILogEntryFilter> CreateFilters(string substringFilter,
		                                                  bool ignoreCase,
		                                                  LevelFlags levelFilter)
		{
			var filters = new List<ILogEntryFilter>();
			if (!string.IsNullOrEmpty(substringFilter))
				filters.Add(new SubstringFilter(substringFilter, ignoreCase));
			if (levelFilter != LevelFlags.All)
				filters.Add(new LevelFilter(levelFilter));
			return filters;
		}

		public static ILogEntryFilter Create(LevelFlags levelFilter,
		                                     IEnumerable<ILogEntryFilter> additionalFilters = null)
		{
			var filters = new List<ILogEntryFilter> {new LevelFilter(levelFilter)};
			if (additionalFilters != null)
				filters.AddRange(additionalFilters);
			return Create(filters);
		}

		public static ILogEntryFilter Create(string substringFilter,
		                                     bool ignoreCase,
		                                     LevelFlags levelFilter,
		                                     IEnumerable<ILogEntryFilter> additionalFilters = null)
		{
			List<ILogEntryFilter> filters = CreateFilters(substringFilter, ignoreCase, levelFilter);
			if (additionalFilters != null)
				filters.AddRange(additionalFilters);
			return Create(filters);
		}
	}
}