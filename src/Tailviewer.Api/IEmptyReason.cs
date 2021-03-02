using System.Collections.Generic;
using System.Windows.Media;

namespace Tailviewer.Api
{
	/// <summary>
	///     Describes why an <see cref="ILogSource" /> is empty.
	/// </summary>
	public interface IEmptyReason
	{
		/// <summary>
		///     A simple, one sentence reason that explains the problem to the end user.
		/// </summary>
		/// <remarks>
		///     Should always be set.
		/// </remarks>
		/// <example>
		///     "The file cannot be accessed"
		/// </example>
		string Reason { get; }

		/// <summary>
		///     An optional, more detailed explanation of the problem. May be multiple sentences, but should
		///     still be kept as short as possible as to not overwhelm the user.
		/// </summary>
		/// <example>
		///     "The file 'C:\foo\bar.log' is locked by another processed and cannot be displayed"
		/// </example>
		string Explanation { get; }

		/// <summary>
		///     An optional list of options presented to the end user to try to solve this problem. Each option
		///     should be no longer than a single sentence (if at all) and give the user a chance to solve the
		///     problem.
		/// </summary>
		/// <remarks>
		///     Each option should be a fragment which follows the word "Try"
		/// </remarks>
		/// <example>
		///     "closing the application which is locking the file"
		///     "restarting your computer"
		/// </example>
		IReadOnlyList<string> Options { get; }

		/// <summary>
		///     An optional icon which is displayed next to the <see cref="Reason" />.
		/// </summary>
		Geometry Icon { get; }
	}
}