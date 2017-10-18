using System.Collections.Generic;
using Tailviewer.Settings;

namespace Tailviewer.Ui.ViewModels
{
	/// <summary>
	///     The interface for a view model which aggregates one or more child data sources itself.
	/// </summary>
	public interface IMergedDataSourceViewModel
		: IDataSourceViewModel
	{
		/// <summary>
		///     Defines how the actual source of each log line is displayed:
		///     Either the filename or a two digit character code is rendered next to each line.
		/// </summary>
		DataSourceDisplayMode DisplayMode { get; set; }

		/// <summary>
		///     The list of sources which are part of this merged data source.
		/// </summary>
		IReadOnlyList<IDataSourceViewModel> Observable { get; }
	}
}