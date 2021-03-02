using System.Text;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Api
{
	/// <summary>
	/// </summary>
	public interface ICustomLogFileFormat
	{
		/// <summary>
		///     The human readable name of this format
		/// </summary>
		string Name { get; }

		/// <summary>
		///     The format (input by a human) which specifies how the log file is to be interpreted
		/// </summary>
		string Format { get; }

		/// <summary>
		///     The encoding with which the text file is to be opened.
		/// </summary>
		Encoding Encoding { get; }
	}
}