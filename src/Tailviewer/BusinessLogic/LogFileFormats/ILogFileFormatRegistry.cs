using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings.CustomFormats;

namespace Tailviewer.BusinessLogic.LogFileFormats
{
	public interface ILogFileFormatRegistry
		: ILogFileFormatRepository
	{
		void Add(CustomLogFileFormat customFormat, ILogFileFormat format);
		void Remove(CustomLogFileFormat customFormat);
		void Replace(CustomLogFileFormat oldCustomFormat, CustomLogFileFormat newCustomFormat, ILogFileFormat newFormat);
	}
}