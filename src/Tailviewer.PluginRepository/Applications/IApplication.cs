using System.IO;

namespace Tailviewer.PluginRepository.Applications
{
	public interface IApplication<in T>
	{
		ExitCode Run(IFilesystem filesystem, IInternalPluginRepository repository, T options);
	}
}