using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using IsabelDb;
using log4net;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Archiver.Repository;
using Tailviewer.PluginRepository.Entities;
using Tailviewer.PluginRepository.Exceptions;

namespace Tailviewer.PluginRepository
{
	public sealed class PluginRepository
		: IPluginRepository
		, IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IFilesystem _filesystem;
		private readonly IDatabase _database;
		private readonly IsabelDb.IDictionary<PluginIdentifier, byte[]> _pluginsById;
		private readonly IsabelDb.IDictionary<PluginIdentifier, PublishedPlugin> _pluginRequirements;
		private readonly IsabelDb.IDictionary<string, User> _users;
		private readonly IsabelDb.IDictionary<Guid, string> _usernamesByAccessToken;

		public PluginRepository(IFilesystem filesystem)
			: this(filesystem, OpenDatabase(Constants.PluginDatabaseFilePath))
		{}

		private static IDatabase OpenDatabase(string fileName)
		{
			Log.InfoFormat("Opening plugin database '{0}'...", fileName);
			return Database.OpenOrCreate(fileName, CustomTypes);
		}

		public PluginRepository(IFilesystem filesystem, IDatabase database)
		{
			_filesystem = filesystem;
			_database = database;
			_users = _database.GetOrCreateDictionary<string, User>("Users");
			_usernamesByAccessToken = _database.GetOrCreateDictionary<Guid, string>("UsersByAccessToken");
			_pluginsById = _database.GetOrCreateDictionary<PluginIdentifier, byte[]>("PluginsById");
			_pluginRequirements = _database.GetOrCreateDictionary<PluginIdentifier, PublishedPlugin>("PluginRequirements");
		}

		public Guid AddUser(string username, string email)
		{
			using (var transaction = _database.BeginTransaction())
			{
				if (_users.ContainsKey(username))
					throw new CannotAddUserException($"The user '{username}' already exists and cannot be modified.");

				const string pattern = "^[a-zA-Z_][a-zA-Z0-9_]*$";
				var regex = new Regex(pattern, RegexOptions.Compiled);
				if (!regex.IsMatch(username))
					throw new CannotAddUserException($"The username '{username}' does not match the expected pattern: {pattern}");

				var accessToken = Guid.NewGuid();
				var user = new User
				{
					Username = username,
					Email = email,
					AccessToken = accessToken
				};
				_users.Put(username, user);
				_usernamesByAccessToken.Put(accessToken, username);

				transaction.Commit();

				return accessToken;
			}
		}

		public void RemoveUser(string username)
		{
			using (var transaction = _database.BeginTransaction())
			{
				if (!_users.TryGet(username, out var user))
					throw new CannotRemoveUserException($"The user '{username}' does not exist.");

				_users.Remove(username);
				_usernamesByAccessToken.Remove(user.AccessToken);

				transaction.Commit();
			}
		}

		public IEnumerable<User> GetAllUsers()
		{
			return _users.GetAllValues();
		}

		public void AddPlugin(string fileName, string accessToken)
		{
			Log.InfoFormat("Adding plugin '{0}' to repository...", fileName);

			byte[] plugin;
			try
			{
				plugin = _filesystem.ReadAllBytes(fileName);
			}
			catch (DirectoryNotFoundException e)
			{
				throw new CannotAddPluginException($"Unable to add plugin: {e.Message}", e);
			}

			var pluginIndex = ReadDescription(plugin);
			var id = new PluginIdentifier(pluginIndex.Id, pluginIndex.Version);

			if (!Guid.TryParse(accessToken, out var token))
				throw new CannotAddPluginException($"'{accessToken}' is not a valid access token.");

			using (var transaction = _database.BeginTransaction())
			{
				if (!_usernamesByAccessToken.TryGet(token, out var userName))
					throw new CannotAddPluginException($"'{accessToken}' is not a valid access token.");

				// TODO: Only throw a temper tantrum in case the plugin to be added differs from the plugin stored in this repository
				if (_pluginRequirements.ContainsKey(id) || _pluginsById.ContainsKey(id))
					throw new CannotAddPluginException($"The plugin '{id}' already exists and cannot be modified.");

				var publishedPlugin = new PublishedPlugin
				{
					User = userName,
					Identifier = id,
					RequiredInterfaces = pluginIndex.ImplementedPluginInterfaces
						.Select(x => new PluginInterface(x.InterfaceTypename, x.InterfaceVersion)).ToList()
				};
				_pluginRequirements.Put(id, publishedPlugin);
				_pluginsById.Put(id, plugin);

				transaction.Commit();
				Log.InfoFormat("Added plugin '{0}' to repository!", fileName);
			}
		}

		public long CountPlugins()
		{
			return _pluginsById.Count();
		}

		public void RemovePlugin(string id, string version)
		{
			Log.InfoFormat("Removing plugin {0} v{1}...", id, version);

			if (!Version.TryParse(version, out var v))
				throw new CannotRemovePluginException($"'{version}' is not a valid version.");

			using (var transaction = _database.BeginTransaction())
			{
				var identifier = new PluginIdentifier(id, v);
				if (!_pluginsById.Remove(identifier))
					throw new CannotRemovePluginException($"No plugin {id} v{version} exists.");

				_pluginRequirements.Remove(identifier);

				transaction.Commit();

				Log.InfoFormat("Removed plugin {0} v{1}", id, version);
			}
		}

		#region Implementation of IPluginRepository

		public IReadOnlyList<PluginIdentifier> FindAllPluginsFor(IReadOnlyList<PluginInterface> interfaces)
		{
			Log.InfoFormat("Retrieving all plugins implementing '{0}'...", string.Join(", ", interfaces));

			var interfacesByName = CreateInterfaceMap(interfaces);

			var plugins = new List<PluginIdentifier>();
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

		public IReadOnlyList<PluginIdentifier> FindAllPlugins()
		{
			Log.InfoFormat("Retrieving all plugins...");

			var plugins = _pluginsById.GetAllKeys().ToList();

			Log.InfoFormat("Found {0} plugins, sending to client...", plugins.Count);

			return plugins;
		}

		public byte[] DownloadPlugin(PluginIdentifier pluginId)
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
		private static bool IsSupported(PublishedPlugin pluginRequirements,
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
					typeof(User),
					typeof(PluginIdentifier),
					typeof(PluginInterface),
					typeof(PublishedPlugin)
				};
			}
		}

		private IPluginPackageIndex ReadDescription(byte[] plugin)
		{
			using (var stream = new MemoryStream(plugin))
			{
				return PluginArchive.OpenRead(stream).Index;
			}
		}
	}
}