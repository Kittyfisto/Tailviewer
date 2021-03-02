using System.Text;

namespace Tailviewer.Api
{
	/// <summary>
	///     Describes a log file format to tailviewer.
	/// </summary>
	/// <remarks>
	///     Once a log file has been positively identified to be of a particular format,
	///     tailviewer will start to use find plugins targeting that format.
	/// </remarks>
	/// <remarks>
	///     Plugins can introduce new formats by simply implementing this interface.
	/// </remarks>
	public interface ILogFileFormat
	{
		/// <summary>
		///     A human readable name of this format.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     A human readable description (one or more sentences) of this format.
		/// </summary>
		string Description { get; }

		/// <summary>
		///     When set to true, then the log file is to be interpreted as a text file.
		/// </summary>
		bool IsText { get; }

		/// <summary>
		///     Describes the expected encoding of the log file. When set to null, then
		///     tailviewer will guess the correct encoding.
		///     Is only used when <see cref="IsText" /> is set to true.
		/// </summary>
		Encoding Encoding { get; }
	}
}