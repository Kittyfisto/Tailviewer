using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using IsabelDb;
using log4net;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Archiver.Registry;

namespace Tailviewer.PluginRegistry
{
	public sealed class PluginRepository
		: IPluginRepository
		, IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IDatabase _database;
		private readonly IsabelDb.IDictionary<PluginRegistryId, byte[]> _pluginsById;
		private readonly IsabelDb.IDictionary<PluginRegistryId, PluginRequirements> _pluginRequirements;

		public PluginRepository()
			: this(OpenDatabase(Constants.PluginDatabaseFilePath))
		{}

		private static IDatabase OpenDatabase(string fileName)
		{
			Log.InfoFormat("Opening plugin database '{0}'...", fileName);
			return Database.OpenOrCreate(fileName, CustomTypes);
		}

		public PluginRepository(IDatabase database)
		{
			_database = database;
			_pluginsById = _database.GetDictionary<PluginRegistryId, byte[]>("PluginsById");
			_pluginRequirements = _database.GetDictionary<PluginRegistryId, PluginRequirements>("PluginRequirements");
		}

		public void AddPlugin(string fileName)
		{
			Log.InfoFormat("Adding plugin '{0}' to repository...", fileName);

			var plugin = File.ReadAllBytes(fileName);
			var pluginIndex = ReadDescription(plugin);
			var id = new PluginRegistryId(pluginIndex.Id, pluginIndex.Version);
			var requirements = new PluginRequirements(pluginIndex.ImplementedPluginInterfaces);

			_pluginRequirements.Put(id, requirements);
			_pluginsById.Put(id, plugin);

			Log.InfoFormat("Successfully added plugin '{0}' to repository!", fileName);
		}

		private IPluginPackageIndex ReadDescription(byte[] plugin)
		{
			using (var stream = new MemoryStream(plugin))
			{
				return PluginArchive.OpenRead(stream).Index;
			}
		}

		#region Implementation of IPluginRepository

		public IReadOnlyList<PluginRegistryId> FindAllPluginsFor(IReadOnlyList<PluginInterface> interfaces)
		{
			Log.InfoFormat("Retrieving all plugins implementing '{0}'...", string.Join(", ", interfaces));

			var interfacesByName = CreateInterfaceMap(interfaces);

			var plugins = new List<PluginRegistryId>();
			foreach (var pair in _pluginRequirements.GetAll())
			{
				if (IsSupported(pair.Value, interfacesByName))
				{
					plugins.Add(pair.Key);
				}
			}

			Log.InfoFormat("Found {0} plugins, sending to client...", plugins.Count);

			return plugins;
		}

		public byte[] DownloadPlugin(PluginRegistryId pluginId)
		{
			Log.InfoFormat("Retrieving plugin '{0}'...", pluginId);

			try
			{
				var pluginContent = _pluginsById.Get(pluginId);

				Log.InfoFormat("Retrieved plugin '{0}' from disk, {1} bytes, sending to client...",
				               pluginId, pluginContent.Length);

				return pluginContent;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught exception while trying to retrieve plugin '{0}':\r\n{1}",
				                pluginId,
				                e);
				// TODO: Throw different exception
				throw;
			}
		}

		#endregion

		#region Implementation of IDisposable

		public void Dispose()
		{
			_database.Dispose();
		}

		#endregion

		[Pure]
		private static Dictionary<string, int> CreateInterfaceMap(IReadOnlyList<PluginInterface> interfaces)
		{
			var map = new Dictionary<string, int>();
			foreach (var @interface in interfaces)
			{
				map.Add(@interface.FullName, @interface.Version);
			}

			return map;
		}

		[Pure]
		private static bool IsSupported(PluginRequirements pluginRequirements,
		                                System.Collections.Generic.IReadOnlyDictionary<string, int> interfaces)
		{
			var requirements = pluginRequirements.RequiredInterfaces;
			if (requirements == null)
				return true;

			foreach (var requirement in requirements)
			{
				if (interfaces.TryGetValue(requirement.FullName, out var actualVersion))
				{
					if (requirement.Version != actualVersion)
						return false;
				}
			}

			return true;
		}

		public static IEnumerable<Type> CustomTypes
		{
			get
			{
				return new[]
				{
					typeof(PluginRegistryId),
					typeof(PluginRequirements)
				};
			}
		}

		public long Count()
		{
			return _pluginsById.Count();
		}
	}
}