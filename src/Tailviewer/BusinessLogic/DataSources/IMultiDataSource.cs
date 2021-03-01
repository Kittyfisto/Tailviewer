using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources
{
	/// <summary>
	///     The interface for a data source which is constructed from one or more child-<see cref="IDataSource" />s:
	///     It provides access to a merged <see cref="ILogSource" /> which contains log entries from each of its
	///     child sources in chronological order.
	/// </summary>
	/// <remarks>
	///     This interface does not define how those childs are added.
	/// </remarks>
	public interface IMultiDataSource
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
		///     Data sources are ordered in a way to match <see cref="IReadOnlyLogEntry.SourceId" />:
		///     If a line has been provided by source 4, then that source is the fourth
		///     entry in this list.
		/// </remarks>
		IReadOnlyList<IDataSource> OriginalSources { get; }

		/// <summary>
		///     Changes the given data source to be either included or excluded from this multi data source.
		/// </summary>
		/// <remarks>
		///     By default, every data source is included, but it can be excluded if desired.
		///     This can be done for a multitude of reasons, but most often it has to do with one source not
		///     containing information relevant to the current analysis, but ungrouping the file is just not
		///     a valid option either.
		/// </remarks>
		/// <param name="dataSource"></param>
		/// <param name="isExcluded"></param>
		void SetExcluded(IDataSource dataSource, bool isExcluded);

		/// <summary>
		///     Tests if the given data source is excluded.
		/// </summary>
		/// <param name="dataSource"></param>
		/// <returns>True when the data source is excluded, false otherwise</returns>
		[Pure]
		bool IsExcluded(IDataSource dataSource);
	}
}