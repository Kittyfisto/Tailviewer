﻿using System.IO;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Api
{
	/// <summary>
	///     Responsible for matching log files to a particular format.
	/// </summary>
	[Service]
	public interface ILogFileFormatMatcher
	{
		/// <summary>
		///     Responsible for detecting the format of a log file with the given filename, content, etc...
		/// </summary>
		/// <remarks>
		///     It is perfectly acceptable to return null when this detector doesn't know what kind of format the file has.
		///     The method will be called again when a change to the file is detected by Tailviewer in which case the
		///     detector gets another chance to get it right.
		/// </remarks>
		/// <remarks>
		///     Any implementation of this method MUST NOT read the entire file into memory. The <see cref="Stream"/> is offered
		///     for those formats where it's necessary to *PEEK* into the file in order to make a proper determination.
		///     Tailviewer will not display anything until this method returns at least once: If this method is implemented in such a
		///     way that it blocks for long amounts of time, then Tailviewer's performance will suffer incredibly so just don't do that.
		/// </remarks>
		/// <param name="fileName">The complete file path of the log file.</param>
		/// <param name="header">A **portion** of the file, usually up to the first 512 bytes</param>
		/// <param name="encoding">
		///     The encoding with which the file should be opened, in case it is a text file.
		/// </param>
		/// <param name="format">The format this matcher has decided the log file is in.</param>
		/// <param name="certainty">
		/// The certainty with which the matcher is sure of its response:
		///  - <see cref="Certainty.None"/> and <see cref="Certainty.Uncertain"/> causes this matcher to be called again when something about the file changes so the matcher may once again attempt to interpret the content
		///  - <see cref="Certainty.Sure"/> causes this matcher to never be bothered again. When the matcher returned a successful match, then this will be the final format the log file has determined to be
		/// </param>
		/// <returns>true in case this matcher is 100% certain that the given log file is of a particular format, false otherwise.</returns>
		[ThreadSafe]
		bool TryMatchFormat(string fileName,
		                    byte[] header,
		                    Encoding encoding,
		                    out ILogFileFormat format,
		                    out Certainty certainty);
	}
}