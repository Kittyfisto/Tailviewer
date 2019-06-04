namespace Tailviewer.BusinessLogic.DataSources
{
	/// <summary>
	///     A data source which watches over a given folder and maintains a list
	///     of child data sources, one for each log file in that folder. This list
	///     is synchronized automatically with that folder.
	/// </summary>
	public interface IFolderDataSource
		: IMultiDataSource
	{
		/// <summary>
		///     The path to the folder which shall be watched over.
		/// </summary>
		string LogFileFolderPath { get; }

		/// <summary>
		///     The regular expression which shall be used to select the log files to display.
		/// </summary>
		string LogFileSearchPattern { get; }

		/// <summary>
		///     When set to true, then <see cref="LogFileFolderPath" /> will be searched recursively for matches against
		///     <see cref="LogFileSearchPattern" />, otherwise only the top-level folder will be searched.
		/// </summary>
		bool Recursive { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="folderPath"></param>
		/// <param name="searchPattern"></param>
		/// <param name="recursive"></param>
		void Change(string folderPath, string searchPattern, bool recursive);
	}
}