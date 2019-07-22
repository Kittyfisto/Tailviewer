using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tailviewer.BusinessLogic.Plugins.Issues
{
	/// <summary>
	///     Represents an issue a <see cref="ILogFileIssueAnalyser" /> has found in a log file.
	/// </summary>
	public interface ILogFileIssue
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
