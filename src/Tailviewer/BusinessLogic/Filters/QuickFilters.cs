using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Core;

namespace Tailviewer.BusinessLogic.Filters
{
	public sealed class QuickFilters
		: IQuickFilters
	{
		private readonly List<QuickFilter> _quickFilters;
		private readonly Core.QuickFiltersSettings _settings;
		private readonly TimeFilter _timeFilter;
		private readonly object _syncRoot;

		public QuickFilters(Core.QuickFiltersSettings settings)
		{
			if (settings == null) throw new ArgumentNullException(nameof(settings));

			_syncRoot = new object();
			_settings = settings;
			_quickFilters = new List<QuickFilter>();
			foreach (Core.QuickFilterSettings setting in settings)
			{
				_quickFilters.Add(new QuickFilter(setting));
			}
			_timeFilter = new TimeFilter(settings.TimeFilter);
		}

		public IEnumerable<QuickFilter> Filters
		{
			get
			{
				lock (_syncRoot)
				{
					return _quickFilters.ToList();
				}
			}
		}

		public TimeFilter TimeFilter => _timeFilter;

		/// <summary>
		///     Adds a new quickfilter.
		/// </summary>
		/// <returns></returns>
		public QuickFilter AddQuickFilter()
		{
			lock (_syncRoot)
			{
				var settings = new Core.QuickFilterSettings();
				var filter = new QuickFilter(settings);
				_quickFilters.Add(filter);
				_settings.Add(settings);
				return filter;
			}
		}

		public void Remove(QuickFilterId id)
		{
			lock (_syncRoot)
			{
				_quickFilters.RemoveAll(x => x.Id == id);
				int idx = _settings.FindIndex(x => Equals(x.Id, id));
				if (idx != -1)
				{
					_settings.RemoveAt(idx);
				}
			}
		}
	}
}