using System;
using System.Collections.Generic;
using System.Linq;

namespace Tailviewer.BusinessLogic
{
	/// <summary>
	/// Combines multiple <see cref="IFilter"/>s into one.
	/// A <see cref="LogLine"/> passes if it passes *all* individual filters.
	/// </summary>
	internal sealed class FilterChain
		: IFilter
	{
		private readonly IFilter[] _filters;

		public FilterChain(IEnumerable<IFilter> filters)
		{
			if (filters == null) throw new ArgumentNullException("filters");

			_filters = filters.ToArray();
			if (_filters.Any(x => x == null)) throw new ArgumentNullException("filters");
		}

		public bool PassesFilter(LogLine logLine)
		{
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable ForCanBeConvertedToForeach
			for (int i = 0; i < _filters.Length; ++i)
// ReSharper restore ForCanBeConvertedToForeach
// ReSharper restore LoopCanBeConvertedToQuery
			{
				if (!_filters[i].PassesFilter(logLine))
					return false;
			}

			return true;
		}
	}
}