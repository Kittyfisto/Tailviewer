using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.DataSources
{
	/// <summary>
	///     The interface for a data source which is constructed from one or more <see cref="IDataSource" />s:
	///     It provides access to a merged <see cref="ILogFile" /> which contains log entries from each of its
	///     child sources in chronological order.
	///     New data sources can be manually added / removed.
	/// </summary>
	public interface IMergedDataSource
		: IMultiDataSource
	{
		/// <summary>
		///     A user defined name for this data source,
		///     will be persisted.
		/// </summary>
		string DisplayName { get; set; }

		/// <summary>
		///     Adds the given data source as a child to this one.
		///     From now on, this data source will represent the aggregate of this child
		///     (and all previously added children).
		/// </summary>
		/// <param name="dataSource"></param>
		void Add(IDataSource dataSource);

		/// <summary>
		///     Removes the given data source from this one.
		/// </summary>
		/// <param name="dataSource"></param>
		void Remove(IDataSource dataSource);
	}
}