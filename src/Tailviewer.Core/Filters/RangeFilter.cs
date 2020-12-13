using System.Collections.Generic;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters
{
	/// <summary>
	/// Responsible for filtering a range of log entries based on their <see cref="LogLine.LineIndex"/>.
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
		public bool PassesFilter(LogLine logLine)
		{
			var index = logLine.LineIndex;
			return !_filteredSection.Contains(index);
		}
		
		/// <inheritdoc />
		public List<LogLineMatch> Match(LogLine line)
		{
			return null;
		}

		#endregion

		#region Implementation of ILogEntryFilter

		/// <inheritdoc />
		public bool PassesFilter(IEnumerable<LogLine> logEntry)
		{
			foreach (var line in logEntry)
			{
				if (!PassesFilter(line))
					return false;
			}

			return true;
		}

		/// <inheritdoc />
		public void Match(LogLine line, List<LogLineMatch> matches)
		{}

		#endregion
	}
}
