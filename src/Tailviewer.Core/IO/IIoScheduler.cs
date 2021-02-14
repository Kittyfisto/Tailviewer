using System;
using System.Text;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Core.IO
{
	/// <summary>
	///     This interface allows read-only, random-access to text files.
	/// </summary>
	[Service]
	public interface IIoScheduler
		: IDisposable
	{
		/// <summary>
		///     Creates a new reader which is able to read data from the given text file both in and out of order.
		/// </summary>
		/// <remarks>
		///     This method never fails, not when the file doesn't exist, not when it isn't readable, not ever.
		/// </remarks>
		/// <param name="fileName"></param>
		/// <param name="listener"></param>
		/// <param name="defaultEncoding"></param>
		/// <param name="formatMatcher"></param>
		/// <returns></returns>
		ITextFileReader OpenReadText(string fileName,
		                             ITextFileListener listener,
		                             Encoding defaultEncoding,
		                             ILogFileFormatMatcher formatMatcher);
	}
}