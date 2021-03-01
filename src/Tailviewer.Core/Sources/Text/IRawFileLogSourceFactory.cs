using System.Text;
using Tailviewer.Core.Columns;

namespace Tailviewer.Core.Sources.Text
{
	/// <summary>
	///     Responsible for opening files in "raw" mode where it's content is interpreted as little as possible.
	/// </summary>
	/// <remarks>
	///     For example, for text log files, the resulting <see cref="ILogSource"/> exposes every line of the text document as
	///     a log entry where the content of the line is exposed through the <see cref="GeneralColumns.RawContent"/> column.
	/// </remarks>
	[Service]
	public interface IRawFileLogSourceFactory
	{
		/// <summary>
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="format"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		ILogSource OpenRead(string fileName, ILogFileFormat format, Encoding encoding);
	}
}