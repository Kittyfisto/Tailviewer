// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	///     Defines how multiple active quick filters are combined with each other.
	/// </summary>
	public enum FilterCombinationMode
	{
		/// <summary>
		///     An entry is shown when it matches every active quick filter.
		/// </summary>
		And = 0,

		/// <summary>
		///     An entry is shown when it matches at least one active quick filter.
		/// </summary>
		Or = 1
	}
}
