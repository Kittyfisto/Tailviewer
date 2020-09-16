using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Plugins
{
	/// <summary>
	///     This plugin, once implemented, allows users to dynamically add log formats to Tailviewer.
	/// </summary>
	/// <remarks>
	///     Tailviewer offers an editor which allows a user to add new custom formats.
	///     This is required for log file formats which give the user great freedom to configure
	///     exactly how a log message is logged (e.g. in which particular format).
	///     Plugins implementing this type should allow the user to specify the format string of the
	///     logging framework these plugins support (e.g. "{Timestamp:dd/MM/yyyy HH:mm:ss K} [{Level}] {Message:lj}" for
	///     a Serilog plugin) and perform the parsing into Tailviewer's <see cref="IReadOnlyLogEntry" />
	///     so Tailviewer is able to interpret the log file.
	/// </remarks>
	public interface ILogFileFormatCreatorPlugin
		: IPlugin
	{
		/// <summary>
		///     The human readable name of log file format implemented by this plugin.
		/// </summary>
		/// <remarks>
		///     Upon creation of a new format, a user uses this information to select which
		///     particular format to add.
		/// </remarks>
		string FormatName { get; }

		/// <summary>
		///     Creates a new log file format of the given type.
		/// </summary>
		/// <remarks>
		///     Tailviewer will register this new format with the <see cref="ILogFileFormatRegistry" />.
		/// </remarks>
		/// <param name="format">The values (entered by a user) from which to create the format.</param>
		/// <returns></returns>
		ILogFileFormat Create(CustomLogFileFormat format);
	}
}