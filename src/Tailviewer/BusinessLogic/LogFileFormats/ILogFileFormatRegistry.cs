using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.LogFileFormats
{
	public interface ILogFileFormatRegistry
		: ILogFileFormatRepository
	{
		void Add(ILogFileFormat format);
		void Remove(ILogFileFormat format);
		void Replace(ILogFileFormat old, ILogFileFormat @new);
	}
}