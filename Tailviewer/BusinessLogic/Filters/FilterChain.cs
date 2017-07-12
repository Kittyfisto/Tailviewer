using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Filters
{
	/// <summary>
	///     Combines multiple <see cref="ILogEntryFilter" />s into one.
	///     A <see cref="LogLine" /> passes if it passes *all* individual filters.
	/// </summary>
	public sealed class FilterChain
		: ILogEntryFilter
	{
		private readonly ILogEntryFilter[] _filters;

		public FilterChain(IEnumerable<ILogEntryFilter> filters)
		{
			if (filters == null) throw new ArgumentNullException(nameof(filters));

			_filters = filters.ToArray();
			if (_filters.Any(x => x == null)) throw new ArgumentNullException(nameof(filters));
		}

		public bool PassesFilter(IEnumerable<LogLine> logEntry)
		{
			var passes = new bool[_filters.Length];
			foreach (LogLine logLine in logEntry)
			{
				for (int i = 0; i < _filters.Length; ++i)
				{
					ILogEntryFilter filter = _filters[i];
					if (!passes[i])
					{
						passes[i] = filter.PassesFilter(logLine);
					}
				}
			}

// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable ForCanBeConvertedToForeach
			for (int i = 0; i < passes.Length; ++i)
// ReSharper restore ForCanBeConvertedToForeach
// ReSharper restore LoopCanBeConvertedToQuery
			{
				if (!passes[i])
					return false;
			}

			return true;
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

		public List<LogLineMatch> Match(LogLine line)
		{
			var ret = new List<LogLineMatch>();
			Match(line, ret);
			return ret;
		}

		public void Match(LogLine line, List<LogLineMatch> matches)
		{
			foreach (var filter in _filters)
			{
				filter.Match(line, matches);
			}
		}
	}
}