using System;

namespace Tailviewer.Api
{
	/// <summary>
	///     This enum describes the possible log levels an <see cref="IReadOnlyLogEntry" /> may be associated with.
	/// </summary>
	[Flags]
	public enum LevelFlags : byte
	{
		/// <summary>
		///     The log line describes a fatal problem.
		///     Log lines with this level are shown to the user in deep red (to indicate its seriousness).
		/// </summary>
		Fatal = 0x01,

		/// <summary>
		///     The log line describes an error.
		///     Log lines with this level are shown to the user in red (to indicate its seriousness).
		/// </summary>
		Error = 0x02,

		/// <summary>
		///     The log line describes a warning.
		///     Log lines with this level are shown to the user in yellow red (to indicate its seriousness).
		/// </summary>
		Warning = 0x04,

		/// <summary>
		///     The log line describes an informational message.
		///     Log lines with this level are shown to the user with a white background.
		/// </summary>
		Info = 0x08,

		/// <summary>
		///     The log line describes a debug/diagnostic message.
		///     Debug messages are assumed to be less important than <see cref="Info"/>, which shows in their default visualization.
		///     Log lines with this level are shown to the user with a white background.
		/// </summary>
		Debug = 0x10,

		/// <summary>
		///     The log line describes a trace message.
		///     Trace messages are assumed to be less important than <see cref="Debug"/>, which shows in their default visualization.
		///     Log lines with this level are shown to the user with a gray foreground, white background.
		/// </summary>
		Trace = 0x20,

		/// <summary>
		///     The log line is not associated with any of the above log levels or it simply
		///     could not be determined.
		/// </summary>
		Other = 0x40,

		/// <summary>
		///     A combination of all possible log levels.
		/// </summary>
		All = Fatal | Error | Warning | Info | Debug | Trace | Other,

		/// <summary>
		///     Not meant to be used directly. Use <see cref="Other"/> instead.
		/// </summary>
		None = 0
	}
}