namespace Tailviewer.Settings
{
	public interface IApplicationSettings
	{
		IAutoUpdateSettings AutoUpdate { get; }
		IMainWindowSettings MainWindow { get; }
		IDataSourcesSettings DataSources { get; }
		IExportSettings Export { get; }
		void SaveAsync();
		bool Save();
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