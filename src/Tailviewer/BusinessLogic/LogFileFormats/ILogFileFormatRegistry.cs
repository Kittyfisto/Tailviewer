using Tailviewer.Settings.CustomFormats;

namespace Tailviewer.BusinessLogic.LogFileFormats
{
	/// <summary>
	/// 
	/// </summary>
	[Service]
	public interface ILogFileFormatRegistry
		: ILogFileFormatRepository
	{
		void Add(CustomLogFileFormat customFormat, ILogFileFormat format);
		void Remove(CustomLogFileFormat customFormat);
		void Replace(CustomLogFileFormat oldCustomFormat, CustomLogFileFormat newCustomFormat, ILogFileFormat newFormat);
	}
}