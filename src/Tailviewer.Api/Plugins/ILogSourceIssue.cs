using System;

namespace Tailviewer.Plugins
{
	/// <summary>
	///     Represents an issue a <see cref="ILogSourceIssueAnalyser" /> has found in a log source.
	/// </summary>
	public interface ILogSourceIssue
	{
		/// <summary>
		///     The **original** log line the issue was found at.
		/// </summary>
		LogLineIndex OriginalLineIndex { get; }

		/// <summary>
		///     The timestamp of when the issue occured.
		/// </summary>
		DateTime? Timestamp { get; }

		/// <summary>
		///     The severity of the issue - more severe issues are presented to the user with more urgency
		/// </summary>
		Severity Severity { get; }

		/// <summary>
		///     A very short summary of the issue, should fit into one line.
		/// </summary>
		string Summary { get; }

		/// <summary>
		///     An optional (very detailed) description of the issue. May span multiple lines.
		/// </summary>
		string Description { get; }
	}
}
