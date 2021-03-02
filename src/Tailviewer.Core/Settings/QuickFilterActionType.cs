// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	///     Defines how a quick filter is to be applied.
	/// </summary>
	public enum QuickFilterActionType
	{
		/// <summary>
		///     Only lines which match the filter are included.
		/// </summary>
		Include = 0,

		/// <summary>
		///     Only lines which do NOT match the filter are included.
		/// </summary>
		Exclude = 1,

		/// <summary>
		///     Lines which match the filter have the matching string colored.
		/// </summary>
		Color = 2
	}
}