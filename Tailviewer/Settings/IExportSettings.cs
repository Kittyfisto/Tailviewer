namespace Tailviewer.Settings
{
	/// <summary>
	///     Holds various settings related to exporting data from tailviewer.
	/// </summary>
	public interface IExportSettings
	{
		/// <summary>
		///     The absolute path to the folder where all exports are stored in.
		/// </summary>
		string ExportFolder { get; set; }
	}
}