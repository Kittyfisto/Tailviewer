using System.Collections.Generic;

namespace Tailviewer.Plugins
{
	/// <summary>
	///     This interface is implemented by tailviewer and expected to be used
	/// by <see cref="ILogSourceIssueAnalyser"/> plugin authors.
	/// </summary>
	public interface ILogFileIssueListener
	{
		/// <summary>
		///    This method must be called by <see cref="ILogSourceIssueAnalyser"/> whenever
		///    its list of issues has changed, for example because a new issue has been added.
		/// </summary>
		/// <remarks>
		///    Tailviewer will make a copy of the given list so plugin authors can simply pass
		///    a reference to their internal list here in order to minimize total the number of copies.
		/// </remarks>
		/// <param name="issues">The complete list of issues</param>
		void OnIssuesChanged(IEnumerable<ILogSourceIssue> issues);
	}
}