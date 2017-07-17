using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.Plugins
{
	public interface IPluginLoader
	{
		T Load<T>(IPluginDescription description) where T : class, IPlugin;
		IEnumerable<T> LoadAllOfType<T>(IEnumerable<IPluginDescription> plugins) where T : class, IPlugin;
	}
}