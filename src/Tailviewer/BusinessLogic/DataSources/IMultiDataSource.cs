using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources
{
	/// <summary>
	///     The interface for a data source which is constructed from one or more child-<see cref="IDataSource" />s:
	///     It provides access to a merged <see cref="ILogFile" /> which contains log entries from each of its
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
		///     Data sources are ordered in a way to match <see cref="LogLine.SourceId" />:
		///     If a line has been provided by source 4, then that source is the fourth
		///     entry in this list.
		/// </remarks>
		IReadOnlyList<IDataSource> OriginalSources { get; }
	}
}