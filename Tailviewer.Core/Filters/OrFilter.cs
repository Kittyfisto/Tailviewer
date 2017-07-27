using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters
{
	public sealed class OrFilter
		: ILogEntryFilter
	{
		private readonly ILogEntryFilter[] _filters;

		public OrFilter(IEnumerable<ILogEntryFilter> filters)
		{
			if (filters == null) throw new ArgumentNullException(nameof(filters));

			_filters = filters.ToArray();
			if (_filters.Any(x => x == null)) throw new ArgumentNullException(nameof(filters));
		}

		/// <inheritdoc />
		public bool PassesFilter(LogLine logLine)
		{
			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < _filters.Length; ++i)
			{
				if (_filters[i].PassesFilter(logLine))
					return true;
			}

			return false;
		}

		/// <inheritdoc />
		public List<LogLineMatch> Match(LogLine line)
		{
			var matches = new List<LogLineMatch>();
			Match(line, matches);
			return matches;
		}

		/// <inheritdoc />
		public bool PassesFilter(IEnumerable<LogLine> logEntry)
		{
			foreach (var logLine in logEntry)
			{
				if (PassesFilter(logLine))
					return true;
			}

			return false;
		}

		/// <inheritdoc />
		public void Match(LogLine line, List<LogLineMatch> matches)
		{
			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < _filters.Length; ++i)
			{
				_filters[i].Match(line, matches);
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			if (_filters.Length == 0)
				return "";

			return string.Join<ILogEntryFilter>(" || ", _filters);
		}
	}
}