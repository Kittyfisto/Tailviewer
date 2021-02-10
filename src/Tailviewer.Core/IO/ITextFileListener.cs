using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.IO
{
	/// <summary>
	/// </summary>
	public interface ITextFileListener
	{
		/// <summary>
		///     This method is invoked whenever the file has been reset / is  no longer available.
		///     The user must assume that all data previously reported via <see cref="OnRead" /> is no longer
		///     available and that the file is empty.
		/// </summary>
		/// <param name="properties">The current properties of the file</param>
		void OnReset(ILogFileProperties properties);

		/// <summary>
		///     This method is called when the reader has reached the end of the file.
		/// </summary>
		/// <param name="properties"></param>
		/// <remarks>
		///     It is possible (and quite likely if the log file is being written at the moment), that after this method is called,
		///     <see cref="OnRead" /> is called again.
		/// </remarks>
		void OnEndOfSourceReached(ILogFileProperties properties);

		/// <summary>
		/// </summary>
		/// <param name="properties"></param>
		/// <param name="readSection"></param>
		/// <param name="readData"></param>
		void OnRead(ILogFileProperties properties, LogFileSection readSection, IReadOnlyList<string> readData);
	}
}