using System.IO;

namespace Tailviewer.PluginRepository.Applications
{
	public interface IApplication<in T>
	{
		int Run(IFilesystem filesystem, IInternalPluginRepository repository, T options);
	}
}