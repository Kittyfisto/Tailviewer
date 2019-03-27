namespace Tailviewer.BusinessLogic.Exporter
{
	public interface ILogFileToFileExporter
		: ILogFileExporter
	{
		string FullExportFilename { get; }
	}
}