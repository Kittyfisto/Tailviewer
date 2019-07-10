namespace Tailviewer.PluginRepository.Applications
{
	public interface IApplication<in T>
	{
		int Run(PluginRepository repository, T options);
	}
}