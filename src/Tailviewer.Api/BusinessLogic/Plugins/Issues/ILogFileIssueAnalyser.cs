using System;

namespace Tailviewer.BusinessLogic.Plugins.Issues
{
	/// <summary>
	///     Responsible for analysing a log file for a list of issues.
	///     Whenever the list of issues changes, an analyser should notify
	///     its <see cref="ILogFileIssueListener" />.
	/// </summary>
	/// <remarks>
	///     The goal of an issue analyser is to **condense** the errors/warnings
	///     of a log file into a **short** and **concise** list of issues
	///     which the user can then inspect.
	/// </remarks>
	public interface ILogFileIssueAnalyser
		: IDisposable
	{
		/// <summary>
		///     Adds a listener to this analyser.
		/// </summary>
		/// <param name="listener">A listener which must be notified whenever the newly created analyzer updates its list of issues</param>
		void AddListener(ILogFileIssueListener listener);

		/// <summary>
		///     Removes a listener from this analyser.
		/// </summary>
		/// <param name="listener"></param>
		void RemoveListener(ILogFileIssueListener listener);

		/// <summary>
		///     This method is called by tailviewer and signals the analyser to start its analysis.
		/// </summary>
		/// <remarks>
		///     Implementations of this interface should NOT call the <see cref="ILogFileIssueListener" /> given by
		///     <see cref="AddListener" /> UNTIl <see cref="Start" /> is called.
		/// </remarks>
		void Start();
	}
}