using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Api;

namespace Tailviewer.Core.Filters
{
	/// <summary>
	///     This filter implements the logical or operation:
	///     The filter matches if any of its child filter match.
	/// </summary>
	internal sealed class OrFilter
		: ILogEntryFilter
	{
		private readonly ILogEntryFilter[] _filters;

		/// <summary>
		///     Initializes this filter.
		/// </summary>
		/// <param name="filters"></param>
		public OrFilter(IEnumerable<ILogEntryFilter> filters)
		{
			if (filters == null) throw new ArgumentNullException(nameof(filters));

			_filters = filters.ToArray();
			if (_filters.Any(x => x == null)) throw new ArgumentNullException(nameof(filters));
		}

		/// <inheritdoc />
		public bool PassesFilter(IReadOnlyLogEntry logLine)
		{
			// ReSharper disable once ForCanBeConvertedToForeach
			for (var i = 0; i < _filters.Length; ++i)
				if (_filters[i].PassesFilter(logLine))
					return true;

			return false;
		}

		/// <inheritdoc />
		public List<LogLineMatch> Match(IReadOnlyLogEntry line)
		{
			var matches = new List<LogLineMatch>();
			Match(line, matches);
			return matches;
		}

		/// <inheritdoc />
		public bool PassesFilter(IEnumerable<IReadOnlyLogEntry> logEntry)
		{
			foreach (var logLine in logEntry)
				if (PassesFilter(logLine))
					return true;

			return false;
		}

		/// <inheritdoc />
		public void Match(IReadOnlyLogEntry line, List<LogLineMatch> matches)
		{
			// ReSharper disable once ForCanBeConvertedToForeach
			for (var i = 0; i < _filters.Length; ++i)
				_filters[i].Match(line, matches);
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