using Tailviewer.BusinessLogic;

namespace Tailviewer.Ui
{
	/// <summary>
	///     This service enables users to navigate tailviewer programmatically.
	///     Is part of <see cref="IServiceContainer" />.
	/// </summary>
	public interface INavigationService
	{
		/// <summary>
		///     Navigates tot he given line of the given data source.
		/// </summary>
		/// <param name="dataSource"></param>
		/// <param name="line"></param>
		bool NavigateTo(DataSourceId dataSource, LogLineIndex line);
	}
}