using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using SharpRemote;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.Archiver.Repository;

namespace Tailviewer.BusinessLogic.Plugins
{
	public sealed class PluginUpdater
		: IPluginUpdater
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IPluginLoader _pluginLoader;

		public PluginUpdater(IPluginLoader pluginLoader)
		{
			_pluginLoader = pluginLoader;
		}

		#region Implementation of IPluginUpdater

		public Task<int> UpdatePluginsAsync(IReadOnlyList<string> repositories)
		{
			return Task.Factory.StartNew(() => UpdatePlugins(repositories));
		}

		public async Task<IReadOnlyList<PublishedPluginDescription>> GetAllPluginsAsync(IReadOnlyList<string> repositories)
		{
			var tasks = new List<Task<IReadOnlyList<PublishedPluginDescription>>>();
			foreach (var repository in repositories)
			{
				tasks.Add(GetAllPluginsAsync(repository));
			}

			await Task.WhenAll(tasks.ToArray());
			var plugins = tasks.SelectMany(x => x.Result).ToList();
			return plugins;
		}

		public Task DownloadPluginAsync(IReadOnlyList<string> repositories, PluginIdentifier plugin)
		{
			return Task.Factory.StartNew(() =>
			{
				Exception lastException = null;
				foreach (var repository in repositories)
				{
					try
					{
						DownloadPlugin(repository, plugin);
						return;
					}
					catch (Exception e)
					{
						lastException = e;
					}
				}

				throw lastException;
			});
		}

		private Task<IReadOnlyList<PublishedPluginDescription>> GetAllPluginsAsync(string repositoryName)
		{
			return Task.Factory.StartNew(() =>
			{
				var endPoint = ResolveRepositoryName(repositoryName);
				using (var client = new SocketEndPoint(EndPointType.Client))
				{
					Connect(client, endPoint);

					var repository = client.CreateProxy<IPluginRepository>(Archiver.Repository.Constants.PluginRepositoryV1Id);
					var interfaces = GetSupportedInterfaces();
					var identifiers = repository.FindAllPluginsFor(interfaces);
					var descriptions = repository.GetDescriptions(identifiers);
					return descriptions;
				}
			});
		}

		#endregion

		private int UpdatePlugins(IReadOnlyList<string> repositories)
		{
			var numUpdated = 0;
			foreach (var repository in repositories)
			{
				numUpdated += UpdatePlugins(repository);
			}

			return numUpdated;
		}

		private int UpdatePlugins(string repository)
		{
			// TODO: This would be the proper place to add support for more protocols, if necessary

			var endPoint = ResolveRepositoryName(repository);
			return UpdatePluginsFromTailviewerRepository(endPoint);
		}

		private static DnsEndPoint ResolveRepositoryName(string repository)
		{
			string prefix = $"{Archiver.Repository.Constants.Protocol}://";
			if (!repository.StartsWith(prefix))
				throw new Exception($"Unsupported protocol: {repository}");

			var uri = repository.Substring(prefix.Length);
			var tokens = uri.Split(':');
			if (tokens.Length != 2)
				throw new Exception($"Expected an address in the form of {prefix}<hostname>:<port>");

			var hostname = tokens[0];
			var port = int.Parse(tokens[1]);
			var endPoint = new DnsEndPoint(hostname, port);
			return endPoint;
		}

		private int UpdatePluginsFromTailviewerRepository(DnsEndPoint endPoint)
		{
			using (var client = new SocketEndPoint(EndPointType.Client))
			{
				Connect(client, endPoint);

				var repository = client.CreateProxy<IPluginRepository>(Archiver.Repository.Constants.PluginRepositoryV1Id);
				return UpdatePlugins(repository);
			}
		}

		private void Connect(SocketEndPoint client, DnsEndPoint endPoint)
		{
			var hostEntry = Dns.GetHostEntry(endPoint.Host);
			var addresses = hostEntry.AddressList;
			if (addresses.Length == 0)
				throw new Exception($"Unable to find host '{endPoint.Host}'");

			Exception lastException = null;
			foreach (var ipAddress in addresses)
				if (client.TryConnect(new IPEndPoint(ipAddress, endPoint.Port), TimeSpan.FromSeconds(10), out lastException, out _))
					return;

			// ReSharper disable once PossibleNullReferenceException
			throw lastException;
		}

		private int UpdatePlugins(IPluginRepository repository)
		{
			var supportedInterfaces = GetSupportedInterfaces();
			var currentPlugins = _pluginLoader.Plugins.Select(x => new PluginIdentifier(x.Id.Value, x.Version)).ToList();
			var allPlugins = repository.FindUpdatesFor(currentPlugins, supportedInterfaces);
			int numUpdated = 0;
			foreach (var installedPlugin in _pluginLoader.Plugins)
			{
				if (TryFindNewestPlugin(installedPlugin, allPlugins, out var id))
				{
					if (TryDownloadAndInstall(repository, id))
					{
						++numUpdated;
					}
				}
			}

			return numUpdated;
		}

		private static IReadOnlyList<PluginInterface> GetSupportedInterfaces()
		{
			var supportedInterfaces = PluginAssemblyLoader
			                          .PluginInterfaces
			                          .Select(x => new PluginInterface(x.FullName,
			                                                           PluginInterfaceVersionAttribute
				                                                           .GetInterfaceVersion(x).Value)).ToList();
			return supportedInterfaces;
		}

		private bool TryFindNewestPlugin(IPluginDescription installedPlugin, IReadOnlyList<PluginIdentifier> allPlugins,
		                                 out PluginIdentifier id)
		{
			var candidates = allPlugins.Where(x => x.Id == installedPlugin.Id.Value)
			                           .OrderByDescending(x => x.Version)
			                           .ToList();
			if (candidates.Count == 0)
			{
				Log.DebugFormat("No newer version for plugin '{0}' available", installedPlugin.Id);

				id = null;
				return false;
			}

			id = candidates[0];
			return true;
		}

		private bool TryDownloadAndInstall(IPluginRepository repository, PluginIdentifier id)
		{
			try
			{
				DownloadAndInstall(repository, id);
				return true;
			}
			catch (Exception e)
			{
				Log.WarnFormat("Unable to download / install plugin '{0}':\r\n{1}", id, e);
				return false;
			}
		}

		private void DownloadAndInstall(IPluginRepository repository, PluginIdentifier id)
		{
			var content = repository.DownloadPlugin(id);
			var fileName = $"{id.Id}.{id.Version}.tvp";
			var filePath = Path.Combine(Constants.DownloadedPluginsPath, fileName);
			var folder = Path.GetDirectoryName(filePath);
			Directory.CreateDirectory(folder);
			File.WriteAllBytes(filePath, content);
		}

		private void DownloadPlugin(string repositoryName, PluginIdentifier plugin)
		{
			var endPoint = ResolveRepositoryName(repositoryName);
			using (var client = new SocketEndPoint(EndPointType.Client))
			{
				Connect(client, endPoint);

				var repository = client.CreateProxy<IPluginRepository>(Archiver.Repository.Constants.PluginRepositoryV1Id);
				DownloadAndInstall(repository, plugin);
			}
		}
	}
}