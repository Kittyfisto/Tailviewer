using System.Collections.Generic;
using Tailviewer.Archiver.Repository;
using Tailviewer.PluginRepository.Configuration;

namespace Tailviewer.PluginRepository.Applications
{
	public sealed class PluginRepositoryProxy
		: IPluginRepository
	{
		private readonly IPluginRepository _inner;
		private readonly ServerConfiguration _configuration;

		public PluginRepositoryProxy(IPluginRepository inner, ServerConfiguration configuration)
		{
			_inner = inner;
			_configuration = configuration;
		}

		#region Implementation of IPluginRepository

		public void PublishPlugin(byte[] plugin, string accessToken)
		{
			if (!_configuration.Publishing.AllowRemotePublish)
				throw new RemotePublishDisabledException();

			_inner.PublishPlugin(plugin, accessToken);
		}

		public IReadOnlyList<PluginIdentifier> FindAllPluginsFor(IReadOnlyList<PluginInterface> interfaces)
		{
			return _inner.FindAllPluginsFor(interfaces);
		}

		public IReadOnlyList<PluginIdentifier> FindUpdatesFor(IReadOnlyList<PluginIdentifier> plugins, IReadOnlyList<PluginInterface> interfaces)
		{
			return _inner.FindUpdatesFor(plugins, interfaces);
		}

		public IReadOnlyList<PluginIdentifier> FindAllPlugins()
		{
			return _inner.FindAllPlugins();
		}

		public IReadOnlyList<PublishedPluginDescription> GetDescriptions(IReadOnlyList<PluginIdentifier> plugins)
		{
			return _inner.GetDescriptions(plugins);
		}

		public IReadOnlyList<byte[]> GetIcons(IReadOnlyList<PluginIdentifier> plugins)
		{
			return _inner.GetIcons(plugins);
		}

		public byte[] DownloadPlugin(PluginIdentifier pluginId)
		{
			return _inner.DownloadPlugin(pluginId);
		}

		#endregion
	}
}