using System.Collections.Generic;

namespace Tailviewer.Core.Filters
{
	/// <summary>
	/// Responsible for filtering a range of log entries based on their <see cref="IReadOnlyLogEntry.Index"/>.
	/// </summary>
	public sealed class RangeFilter
		: ILogEntryFilter
	{
		private readonly LogFileSection _filteredSection;

		/// <summary>
		/// Initializes this range filter.
		/// </summary>
		/// <param name="filteredSection"></param>
		public RangeFilter(LogFileSection filteredSection)
		{
			_filteredSection = filteredSection;
		}

		#region Implementation of ILogLineFilter

		/// <inheritdoc />
		public bool PassesFilter(IReadOnlyLogEntry logLine)
		{
			var index = logLine.Index;
			return !_filteredSection.Contains(index);
		}
		
		/// <inheritdoc />
		public List<LogLineMatch> Match(IReadOnlyLogEntry line)
		{
			return null;
		}

		#endregion

		#region Implementation of ILogEntryFilter

		/// <inheritdoc />
		public bool PassesFilter(IEnumerable<IReadOnlyLogEntry> logEntry)
		{
			foreach (var line in logEntry)
			{
				if (!PassesFilter(line))
					return false;
			}

			return true;
		}

		/// <inheritdoc />
		public void Match(IReadOnlyLogEntry line, List<LogLineMatch> matches)
		{}

		#endregion
	}
}
