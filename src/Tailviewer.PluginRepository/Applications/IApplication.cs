using System.IO;

namespace Tailviewer.PluginRepository.Applications
{
	public interface IApplication<in T>
	{
		bool RequiresRepository { get; }

		bool ReadOnlyRepository { get; }

		ExitCode Run(IFilesystem filesystem, IInternalPluginRepository repository, T options);
	}
}