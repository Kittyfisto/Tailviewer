using System.Collections.Generic;
using Tailviewer.BusinessLogic.Bookmarks;

namespace Tailviewer.BusinessLogic.DataSources
{
	internal interface IDataSources
		: IEnumerable<IDataSource>
	{
		#region Bookmarks

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dataSource"></param>
		/// <param name="orignalLogLineIndex"></param>
		/// <returns></returns>
		Bookmark TryAddBookmark(IDataSource dataSource, LogLineIndex orignalLogLineIndex);

		/// <summary>
		/// 
		/// </summary>
		IReadOnlyList<Bookmark> Bookmarks { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="bookmark"></param>
		void RemoveBookmark(Bookmark bookmark);

		#endregion
	}
}