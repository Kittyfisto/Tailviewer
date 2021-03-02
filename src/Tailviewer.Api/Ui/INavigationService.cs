// ReSharper disable once CheckNamespace
namespace Tailviewer.Api
{
	/// <summary>
	///     This service enables users to navigate tailviewer programmatically.
	///     Is part of <see cref="IServiceContainer" />.
	/// </summary>
	[Service]
	public interface INavigationService
	{
		/// <summary>
		///     Navigates to the given line of the currently selected data source.
		/// </summary>
		/// <param name="line"></param>
		bool NavigateTo(LogLineIndex line);

		/// <summary>
		///     Navigates to the given line of the given data source.
		/// </summary>
		/// <param name="dataSource"></param>
		/// <param name="line"></param>
		bool NavigateTo(DataSourceId dataSource, LogLineIndex line);
	}
}