namespace Tailviewer.BusinessLogic.Plugins
{
	public interface IPluginLoader
	{
		T Load<T>(IPluginDescription description) where T : class, IPlugin;
	}
}