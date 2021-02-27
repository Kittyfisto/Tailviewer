namespace Tailviewer.Ui.DataSourceTree
{
	/// <summary>
	///     The interface for a data source which only represents a single source
	///     (and not multiple, at least not from the POV of tailviewer...).
	/// </summary>
	public interface ISingleDataSourceViewModel
		: IDataSourceViewModel
	{
		/// <summary>
		///     When set to true, a user can remove this data source, otherwise he cannot.
		/// </summary>
		bool CanBeRemoved { get; }

		/// <summary>
		///     The character code unique to this data source amongst all sibling data sources,
		///     i.e. those with the same parent.
		/// </summary>
		string CharacterCode { get; set; }

		/// <summary>
		///     Whether or not this data source is excluded from its parent.
		///     An excluded data source's log events will not be displayed in the parent's view
		///     until it's included again.
		/// </summary>
		bool ExcludeFromParent { get; set; }
	}
}