using System.Collections.Generic;

namespace Tailviewer.Settings.CustomFormats
{
	public interface IDataSourcesSettings
		: IList<DataSource>
	{
		DataSourceId SelectedItem { get; set; }

		/// <summary>
		///     Moves the element <paramref name="dataSource" /> to appear *anchor*
		///     <paramref name="anchor" />. Does nothing if this constraint doesn't
		///     exist of if either are not part of this list.
		/// </summary>
		/// <param name="dataSource"></param>
		/// <param name="anchor"></param>
		void MoveBefore(DataSource dataSource, DataSource anchor);

		/// <summary>
		///     The list of log files (wildcard patterns) which is used to select log
		///     files in any newly created 'folder data source'.
		/// </summary>
		string FolderDataSourcePattern { get; set; }

		/// <summary>
		///     When set to true, then any newly created 'folder data source' will select
		///     log files recursively. When set to false, only log files on the top folder
		///     will be selected.
		/// </summary>
		bool FolderDataSourceRecursive { get; set; }
	}
}