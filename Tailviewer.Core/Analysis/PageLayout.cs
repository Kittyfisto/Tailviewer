using System.Runtime.Serialization;

namespace Tailviewer.Core.Analysis
{
	/// <summary>
	///     Describes the available page layouts (the way widgets are arranged on a page).
	/// </summary>
	[DataContract]
	public enum PageLayout
	{
		/// <summary>
		///     No layout.
		/// </summary>
		[EnumMember] None = 0,

		/// <summary>
		///     Horizontally wrapped layout: Widgets are laid our left to right, top to bottom.
		/// </summary>
		[EnumMember] WrapHorizontal = 1,

		/// <summary>
		///     Column layout: Widgets are laid out (equally sized) left to right.
		/// </summary>
		[EnumMember] Columns = 2,

		/// <summary>
		///     Row layout: Widgets are laid out (equally sized) top to bottom.
		/// </summary>
		[EnumMember] Rows = 3
	}
}