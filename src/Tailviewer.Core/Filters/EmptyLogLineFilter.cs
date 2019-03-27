using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters
{
	/// <summary>
	/// A filter responsible for filtering out log lines that are completely empty
	/// (or only have whitespace characters).
	/// </summary>
	public sealed class EmptyLogLineFilter
		: ILogLineFilter
	{
		/// <inheritdoc />
		public bool PassesFilter(LogLine logLine)
		{
			var message = logLine.Message;

			return !string.IsNullOrWhiteSpace(message);
		}

		/// <inheritdoc />
		public List<LogLineMatch> Match(LogLine line)
		{
			return new List<LogLineMatch>();
		}
	}
}