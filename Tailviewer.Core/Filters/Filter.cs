using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic;
using Tailviewer.Core.Settings;

namespace Tailviewer.Core.Filters
{
	/// <summary>
	///     This class serves as a collection of named constructors to create various <see cref="ILogEntryFilter"/>s.
	/// </summary>
	public static class Filter
	{
		public static ILogEntryFilter Create(IEnumerable<ILogEntryFilter> filters)
		{
			var tmp = filters.Where(x => x != null).ToList();
			if (tmp.Count == 0)
				return null;

			if (tmp.Count == 1)
				return tmp[0];
			return new AndFilter(tmp);
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
			IEnumerable<ILogEntryFilter> andFilters = null,
			IEnumerable<ILogEntryFilter> orFilters = null)
		{
			var filters = new List<ILogEntryFilter> {new LevelFilter(levelFilter)};
			if (andFilters != null)
				filters.AddRange(andFilters);
			return Create(filters);
		}

		public static ILogEntryFilter Create(string substringFilter,
			bool ignoreCase,
			LevelFlags levelFilter,
			IEnumerable<ILogEntryFilter> additionalFilters = null)
		{
			var filters = CreateFilters(substringFilter, ignoreCase, levelFilter);
			if (additionalFilters != null)
				filters.AddRange(additionalFilters);
			return Create(filters);
		}
	}
}