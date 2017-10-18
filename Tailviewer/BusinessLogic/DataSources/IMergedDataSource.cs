using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources
{
	/// <summary>
	///     The interface for a data source which is constructed from one or more <see cref="IDataSource" />s:
	///     It provides access to a merged <see cref="ILogFile" /> which contains log entries from each of its
	///     child sources in chronological order.
	/// </summary>
	public interface IMergedDataSource
		: IDataSource
	{
		/// <summary>
		///     True when the individual sources of this merged data source are expanded (and thus visible), false
		///     otherwise.
		/// </summary>
		bool IsExpanded { get; set; }

		/// <summary>
		///     The mode the original data source shall be displayed per line.
		/// </summary>
		DataSourceDisplayMode DisplayMode { get; set; }

		/// <summary>
		///     The data sources which make up this merged data source.
		/// </summary>
		/// <remarks>
		///     Data sources are ordered in a way to match <see cref="LogLine.SourceId" />:
		///     If a line has been provided by source 4, then that source is the fourth
		///     entry in this list.
		/// </remarks>
		IReadOnlyList<IDataSource> OriginalSources { get; }

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