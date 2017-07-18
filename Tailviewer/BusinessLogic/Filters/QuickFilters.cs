using System;
using System.Collections.Generic;
using System.Linq;

namespace Tailviewer.BusinessLogic.Filters
{
	public sealed class QuickFilters : IQuickFilters
	{
		private readonly List<QuickFilter> _quickFilters;
		private readonly Core.Settings.QuickFilters _settings;
		private readonly object _syncRoot;

		public QuickFilters(Core.Settings.QuickFilters settings)
		{
			if (settings == null) throw new ArgumentNullException(nameof(settings));

			_syncRoot = new object();
			_settings = settings;
			_quickFilters = new List<QuickFilter>();
			foreach (Core.Settings.QuickFilter setting in settings)
			{
				_quickFilters.Add(new QuickFilter(setting));
			}
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

		/// <summary>
		///     Adds a new quickfilter.
		/// </summary>
		/// <returns></returns>
		public QuickFilter Add()
		{
			lock (_syncRoot)
			{
				var settings = new Core.Settings.QuickFilter();
				var filter = new QuickFilter(settings);
				_quickFilters.Add(filter);
				_settings.Add(settings);
				return filter;
			}
		}

		public void Remove(Guid id)
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