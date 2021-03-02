using Tailviewer.Core;
using Tailviewer.Settings.CustomFormats;

namespace Tailviewer.Settings
{
	/// <summary>
	/// 
	/// </summary>
	public interface IApplicationSettings
	{
		/// <summary>
		/// 
		/// </summary>
		IAutoUpdateSettings AutoUpdate { get; }

		/// <summary>
		/// 
		/// </summary>
		IMainWindowSettings MainWindow { get; }

		/// <summary>
		/// 
		/// </summary>
		IDataSourcesSettings DataSources { get; }

		/// <summary>
		/// 
		/// </summary>
		ILogViewerSettings LogViewer { get; }

		/// <summary>
		/// 
		/// </summary>
		IExportSettings Export { get; }

		/// <summary>
		/// 
		/// </summary>
		ILogFileSettings LogFile { get; }

		/// <summary>
		/// 
		/// </summary>
		ICustomFormatsSettings CustomFormats { get; }

		/// <summary>
		/// 
		/// </summary>
		void SaveAsync();

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		bool Save();

		/// <summary>
		/// 
		/// </summary>
		void Restore();

		/// <summary>
		/// </summary>
		/// <param name="neededPatching">
		///     Whether or not certain values need to be changed (for example due to upgrades to the
		///     format - it is advised that the current settings be saved again if this is set to true)
		/// </param>
		void Restore(out bool neededPatching);
	}
}