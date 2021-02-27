using Tailviewer.Ui.LogView.DataSource;

namespace Tailviewer.Settings
{
	/// <summary>
	///     Defines how <see cref="DataSourceCanvas" /> displays individual data sources.
	/// </summary>
	public enum DataSourceDisplayMode
	{
		/// <summary>
		///     The filename of a data source is displayed (up to a certain character limit).
		/// </summary>
		Filename,

		/// <summary>
		///     A distinct one or two letter character code is displayed.
		/// </summary>
		CharacterCode
	}
}