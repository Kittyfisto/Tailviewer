using System.Text;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Settings
{
	/// <summary>
	///     Responsible for storing settings related to <see cref="ILogFile" /> and its implementations.
	/// </summary>
	public interface ILogFileSettings
	{
		/// <summary>
		///     The encoding used to interpret text based log files.
		/// </summary>
		/// <remarks>
		///     When no encoding is specified (=null), which is the default,
		///     then the encoding will be auto detected and/or specified by plugins.
		///     When a non-null value is specified, then that value will be used
		///     for all text based log files, no matter what.
		/// </remarks>
		Encoding DefaultEncoding { get; set; }
	}
}