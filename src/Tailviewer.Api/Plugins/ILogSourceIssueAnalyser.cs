using System;

namespace Tailviewer.Plugins
{
	/// <summary>
	///     Responsible for analysing a log file for a list of issues.
	///     Whenever the list of issues changes, an analyzer should notify
	///     its <see cref="ILogFileIssueListener" />.
	/// </summary>
	/// <remarks>
	///     The goal of an issue analyzer is to **condense** the errors/warnings
	///     of a log file into a **short** and **concise** list of issues
	///     which the user can then inspect.
	/// </remarks>
	public interface ILogSourceIssueAnalyser
		: IDisposable
	{
		/// <summary>
		///     Adds a listener to this analyzer.
		/// </summary>
		/// <param name="listener">A listener which must be notified whenever the newly created analyzer updates its list of issues</param>
		void AddListener(ILogFileIssueListener listener);

		/// <summary>
		///     Removes a listener from this analyzer.
		/// </summary>
		/// <param name="listener"></param>
		void RemoveListener(ILogFileIssueListener listener);

		/// <summary>
		///     This method is called by tailviewer and signals the analyzer to start its analysis.
		/// </summary>
		/// <remarks>
		///     Implementations of this interface should NOT call the <see cref="ILogFileIssueListener" /> given by
		///     <see cref="AddListener" /> UNTIl <see cref="Start" /> is called.
		/// </remarks>
		void Start();
	}
}