using System.Collections.Generic;
using Tailviewer.BusinessLogic.Bookmarks;

namespace Tailviewer.BusinessLogic.DataSources
{
	public interface IDataSources
	{
		#region Bookmarks

		/// <summary>
		/// </summary>
		/// <param name="dataSource"></param>
		/// <param name="orignalLogLineIndex"></param>
		/// <returns></returns>
		Bookmark TryAddBookmark(IDataSource dataSource, LogLineIndex orignalLogLineIndex);

		/// <summary>
		/// </summary>
		IReadOnlyList<Bookmark> Bookmarks { get; }

		/// <summary>
		/// </summary>
		/// <param name="bookmark"></param>
		void RemoveBookmark(Bookmark bookmark);

		#endregion

		#region Datasources

		IReadOnlyList<IDataSource> Sources { get; }

		/// <summary>
		///     Tests if a data source with the given id exists.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		bool Contains(DataSourceId id);

		SingleDataSource AddFile(string fileName);
		IFolderDataSource AddFolder(string folderPath);
		MergedDataSource AddGroup();
		bool Remove(IDataSource viewModelDataSource);

		#endregion
	}
}