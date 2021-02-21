using System.Collections.Generic;
using System.Linq;
using Tailviewer.Core.Settings;

namespace Tailviewer.Core.Filters
{
	/// <summary>
	///     This class serves as a collection of named constructors to create various <see cref="ILogEntryFilter" />s.
	/// </summary>
	public static class Filter
	{
		/// <summary>
		///     Creates a new <see cref="ILogEntryFilter" /> from the given values.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="matchType"></param>
		/// <param name="ignoreCase"></param>
		/// <param name="isInverted"></param>
		/// <returns></returns>
		public static ILogEntryFilter Create(string value, FilterMatchType matchType, bool ignoreCase, bool isInverted)
		{
			ILogEntryFilter filter = null;
			switch (matchType)
			{
				case FilterMatchType.SubstringFilter:
					if (!string.IsNullOrEmpty(value))
						filter = new SubstringFilter(value, ignoreCase);
					break;

				case FilterMatchType.WildcardFilter:
					if (!string.IsNullOrEmpty(value))
						filter = new WildcardFilter(value, ignoreCase);
					break;

				case FilterMatchType.RegexpFilter:
					if (!string.IsNullOrEmpty(value))
						filter = new RegexFilter(value, ignoreCase);
					break;
			}

			if (filter != null && isInverted)
				filter = new InvertFilter(filter);

			return filter;
		}

		/// <summary>
		///     Creates a new <see cref="ILogLineFilter"/> from the given values.
		/// </summary>
		/// <param name="filters"></param>
		/// <returns></returns>
		public static ILogLineFilter Create(IEnumerable<ILogLineFilter> filters)
		{
			var tmp = filters.Where(x => x != null).Select(x => (ILogEntryFilter)x).ToList();
			return Create(tmp);
		}

		/// <summary>
		///     Creates a new <see cref="ILogEntryFilter" /> from the given values.
		/// </summary>
		/// <param name="filters"></param>
		/// <returns></returns>
		public static ILogEntryFilter Create(IEnumerable<ILogEntryFilter> filters)
		{
			var tmp = filters.Where(x => x != null).ToList();
			if (tmp.Count == 0)
				return null;
			if (tmp.Count == 1)
			{
				var filter = tmp[0];
				if (filter is NoFilter)
					return null;
				return filter;
			}
			return new AndFilter(tmp);
		}

		/// <summary>
		///     Creates a new <see cref="ILogEntryFilter" /> from the given values.
		/// </summary>
		/// <param name="substringFilter"></param>
		/// <returns></returns>
		public static ILogEntryFilter Create(string substringFilter)
		{
			var filter = new SubstringFilter(substringFilter, ignoreCase: true);
			return filter;
		}

		/// <summary>
		///     Creates a new <see cref="ILogEntryFilter" /> from the given values.
		/// </summary>
		/// <param name="substringFilter"></param>
		/// <param name="levelFilter"></param>
		/// <returns></returns>
		public static ILogEntryFilter Create(string substringFilter,
			LevelFlags levelFilter)
		{
			return Create(CreateFilters(substringFilter, ignoreCase: true, levelFilter: levelFilter));
		}

		/// <summary>
		///     Creates a new <see cref="ILogEntryFilter" /> from the given values.
		/// </summary>
		/// <param name="substringFilter"></param>
		/// <param name="ignoreCase"></param>
		/// <param name="levelFilter"></param>
		/// <returns></returns>
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="levels"></param>
		/// <returns></returns>
		public static ILogEntryFilter Create(LevelFlags levels)
		{
			if (levels == LevelFlags.All)
				return null;
			return new LevelFilter(levels);
		}

		/// <summary>
		///     Creates a new <see cref="ILogEntryFilter" /> from the given values.
		/// </summary>
		/// <param name="levelFilter"></param>
		/// <param name="andFilters"></param>
		/// <param name="orFilters"></param>
		/// <returns></returns>
		public static ILogEntryFilter Create(LevelFlags levelFilter,
			IEnumerable<ILogEntryFilter> andFilters,
			IEnumerable<ILogEntryFilter> orFilters = null)
		{
			var filters = new List<ILogEntryFilter> {Create(levelFilter)};
			if (andFilters != null)
				filters.AddRange(andFilters);
			return Create(filters);
		}

		/// <summary>
		///     Creates a new <see cref="ILogEntryFilter" /> from the given values.
		/// </summary>
		/// <param name="substringFilter"></param>
		/// <param name="ignoreCase"></param>
		/// <param name="levelFilter"></param>
		/// <param name="additionalFilters"></param>
		/// <returns></returns>
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logEntryFilter"></param>
		/// <returns></returns>
		public static bool IsFilter(ILogLineFilter logEntryFilter)
		{
			if (logEntryFilter == null)
				return false;

			if (logEntryFilter is NoFilter)
				return false;

			return true;
		}
	}
}