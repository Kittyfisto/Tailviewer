using System;

namespace Tailviewer.BusinessLogic.Plugins.Issues
{
	/// <summary>
	///     Represents an issue a <see cref="ILogFileIssueAnalyser" /> has found in a log file.
	/// </summary>
	public sealed class LogFileIssue
	{
		/// <summary>
		///     Initializes this issue.
		/// </summary>
		/// <param name="line">The **original** log line the issue was found at.</param>
		/// <param name="timestamp">The timestamp of when the issue occured.</param>
		/// <param name="severity">The severity of the issue - more severe issues are presented to the user with more urgency</param>
		/// <param name="summary">A very short summary of the issue, should fit into one line.</param>
		/// <param name="description">An optional (very detailed) description of the issue. May span multiple lines.</param>
		public LogFileIssue(LogLineIndex line, DateTime? timestamp, Severity severity, string summary, string description)
		{
			Line = line;
			Timestamp = timestamp;
			Severity = severity;
			Summary = summary;
			Description = description;
		}

		/// <summary>
		///     The **original** log line the issue was found at.
		/// </summary>
		public LogLineIndex Line { get; }

		/// <summary>
		///     The timestamp of when the issue occured.
		/// </summary>
		public DateTime? Timestamp { get; } 

		/// <summary>
		///     The severity of the issue - more severe issues are presented to the user with more urgency
		/// </summary>
		public Severity Severity { get; }

		/// <summary>
		///     A very short summary of the issue, should fit into one line.
		/// </summary>
		public string Summary { get; }

		/// <summary>
		///     An optional (very detailed) description of the issue. May span multiple lines.
		/// </summary>
		public string Description { get; }
	}
}