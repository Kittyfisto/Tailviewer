using System.Text;

namespace Tailviewer.BusinessLogic.Plugins
{
	/// <summary>
	/// </summary>
	public struct CustomLogFileFormat
	{
		/// <summary>
		///     The human readable name of this format
		/// </summary>
		public string Name;

		/// <summary>
		///     The format (input by a human) which specifies how the log file is to be interpreted
		/// </summary>
		public string Format;

		/// <summary>
		///     The encoding with which the text file is to be opened.
		/// </summary>
		public Encoding Encoding;
	}
}