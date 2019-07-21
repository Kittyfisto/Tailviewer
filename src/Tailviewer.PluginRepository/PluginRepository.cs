using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using IsabelDb;
using log4net;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Archiver.Repository;
using Tailviewer.PluginRepository.Entities;
using Tailviewer.PluginRepository.Exceptions;
using Change = Tailviewer.PluginRepository.Entities.Change;

namespace Tailviewer.PluginRepository
{
	public sealed class PluginRepository
		: IInternalPluginRepository
		, IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IDatabase _database;
		private readonly IsabelDb.IDictionary<PluginIdentifier, byte[]> _plugins;
		private readonly IsabelDb.IDictionary<PluginIdentifier, byte[]> _pluginIcons;
		private readonly IsabelDb.IDictionary<PluginIdentifier, PublishedPlugin> _pluginDescriptions;
		private readonly IsabelDb.IDictionary<string, User> _users;
		private readonly IsabelDb.IDictionary<Guid, string> _usernamesByAccessToken;

		public static PluginRepository Create()
		{
			var database = OpenDatabase(Constants.PluginDatabaseFilePath, out var created);
			return new PluginRepository(database, created);
		}

		private static IDatabase OpenDatabase(string fileName, out bool created)
		{
			Log.DebugFormat("Opening plugin database '{0}'...", fileName);
			if (!File.Exists(fileName))
			{
				created = true;
				return Database.OpenOrCreate(fileName, CustomTypes);
			}

			created = false;
			return Database.Open(fileName, CustomTypes);
		}

		public PluginRepository(IDatabase database, bool newlyCreated)
		{
			_database = database;
			_users = _database.GetOrCreateDictionary<string, User>("Users");
			_usernamesByAccessToken = _database.GetOrCreateDictionary<Guid, string>("UsersByAccessToken");
			_plugins = _database.GetOrCreateDictionary<PluginIdentifier, byte[]>("Plugins");
			_pluginIcons = _database.GetOrCreateDictionary<PluginIdentifier, byte[]>("PluginIcons");
			_pluginDescriptions = _database.GetOrCreateDictionary<PluginIdentifier, PublishedPlugin>("PluginDescriptions");

			if (newlyCreated)
			{
				AddUser("root", "root@home");
			}
		}

		public Guid AddUser(string username, string email)
		{
			Log.DebugFormat("Adding user '{0}, {1}' to repository...", username, email);

			const string pattern = "^[a-zA-Z_][a-zA-Z0-9_]*$";
			var regex = new Regex(pattern, RegexOptions.Compiled);
			if (username == null || !regex.IsMatch(username))
				throw new CannotAddUserException($"The username '{username}' does not match the expected pattern: {pattern}");

			if (!IsValidEmail(email))
				throw new CannotAddUserException($"The email '{email}' does not appear to be a valid email address.");

			using (var transaction = _database.BeginTransaction())
			{
				if (_users.ContainsKey(username))
					throw new CannotAddUserException($"The user '{username}' already exists and cannot be modified.");

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

		public void PublishPlugin(byte[] plugin, string accessToken, string publishTimestamp)
		{
			DateTime publishDate;
			if (!string.IsNullOrEmpty(publishTimestamp))
				publishDate = DateTime.Parse(publishTimestamp, CultureInfo.InvariantCulture);
			else
				publishDate = DateTime.UtcNow;

			PublishPlugin(plugin, accessToken, publishDate);
		}

		public void PublishPlugin(byte[] plugin, string accessToken, DateTime publishDate)
		{
			Log.DebugFormat("Adding plugin ({0} bytes) to repository...", plugin.Length);

			IPluginPackageIndex pluginIndex;
			DateTime builtTime;
			IReadOnlyList<SerializableChange> changes;
			byte[] icon;
			using (var stream = new MemoryStream(plugin))
			using (var archive = OpenPlugin(stream))
			{
				pluginIndex = archive.Index;
				changes = archive.LoadChanges();
				builtTime = GetBuildTime(archive.ReadAssembly());
				icon = archive.ReadIcon()?.ReadToEnd();
			}
			var id = new PluginIdentifier(pluginIndex.Id, pluginIndex.Version);

			if (!Guid.TryParse(accessToken, out var token))
				throw new InvalidUserTokenException($"'{accessToken}' is not a valid access token.");

			using (var transaction = _database.BeginTransaction())
			{
				if (!_usernamesByAccessToken.TryGet(token, out var userName))
					throw new InvalidUserTokenException($"'{accessToken}' is not a valid access token.");

				// TODO: Only throw a temper tantrum in case the plugin to be added differs from the plugin stored in this repository
				if (_pluginDescriptions.ContainsKey(id) || _plugins.ContainsKey(id))
					throw new PluginAlreadyPublishedException($"The plugin '{id}' already exists and cannot be modified.");

				var publishedPlugin = new PublishedPlugin(pluginIndex)
				{
					Publisher = userName,
					Identifier = id,
					BuildDate = builtTime,
					SizeInBytes = plugin.Length,
					PublishDate = publishDate,
					RequiredInterfaces = pluginIndex.ImplementedPluginInterfaces
						.Select(x => new PluginInterface(x.InterfaceTypename, x.InterfaceVersion)).ToList(),
				};
				_pluginDescriptions.Put(id, publishedPlugin);
				_plugins.Put(id, plugin);
				_pluginIcons.Put(id, icon);

				transaction.Commit();
				Log.InfoFormat("Added plugin '{0}' to repository!", id);
			}
		}

		private static PluginArchive OpenPlugin(MemoryStream stream)
		{
			try
			{
				return PluginArchive.OpenRead(stream);
			}
			catch (Exception e)
			{
				Log.WarnFormat("Unable to open plugin: {0}", e);
				throw new CorruptPluginException(e);
			}
		}

		public long CountPlugins()
		{
			return _plugins.Count();
		}

		public void RemovePlugin(string id, string version)
		{
			Log.DebugFormat("Removing plugin {0} v{1}...", id, version);

			if (!Version.TryParse(version, out var v))
				throw new CannotRemovePluginException($"'{version}' is not a valid version.");

			using (var transaction = _database.BeginTransaction())
			{
				var identifier = new PluginIdentifier(id, v);
				if (!_plugins.Remove(identifier))
					throw new CannotRemovePluginException($"No plugin {id} v{version} exists.");

				_pluginDescriptions.Remove(identifier);
				_pluginIcons.Remove(identifier);

				transaction.Commit();

				Log.InfoFormat("Removed plugin {0} v{1}", id, version);
			}
		}

		#region Implementation of IPluginRepository

		public void PublishPlugin(byte[] plugin, string accessToken)
		{
			PublishPlugin(plugin, accessToken, DateTime.UtcNow);
		}

		public IReadOnlyList<PluginIdentifier> FindAllPluginsFor(IReadOnlyList<PluginInterface> interfaces)
		{
			var stopwatch = Stopwatch.StartNew();
			Log.DebugFormat("Retrieving all plugins compatible to '{0}'...", string.Join(", ", interfaces));

			var interfacesByName = CreateInterfaceMap(interfaces);

			// TODO: Use proper indices when necessary...
			var plugins = new List<PluginIdentifier>();
			foreach (var pair in _pluginDescriptions.GetAll())
			{
				if (IsSupported(pair.Value, interfacesByName))
				{
					plugins.Add(pair.Key);
				}
			}

			stopwatch.Stop();
			Log.InfoFormat("Found {0} plugins ({1}) (took {2}ms)", plugins.Count,
				string.Join(", ", plugins),
				stopwatch.ElapsedMilliseconds);

			return plugins;
		}

		public IReadOnlyList<PluginIdentifier> FindUpdatesFor(IReadOnlyList<PluginIdentifier> plugins, IReadOnlyList<PluginInterface> interfaces)
		{
			var stopwatch = Stopwatch.StartNew();
			Log.DebugFormat("Retrieving newer versions of {0} plugin(s) ({1}) compatible to '{2}'...", plugins.Count,
				string.Join(", ", plugins),
				string.Join(", ", interfaces));

			var interfacesByName = CreateInterfaceMap(interfaces);

			// TODO: Use proper indices when necessary...
			var ids = plugins.ToDictionary(x => x.Id, x => x.Version);
			var newerPlugins = new List<PluginIdentifier>();
			foreach (var pair in _pluginDescriptions.GetAll())
			{
				if (ids.TryGetValue(pair.Key.Id, out var currentVersion) &&
					currentVersion < pair.Key.Version &&
				    IsSupported(pair.Value, interfacesByName))
				{
					newerPlugins.Add(pair.Key);
				}
			}

			stopwatch.Stop();

			LogNewerPlugins(plugins, newerPlugins, stopwatch);

			return newerPlugins;
		}

		private static void LogNewerPlugins(IReadOnlyList<PluginIdentifier> plugins, List<PluginIdentifier> newerPlugins, Stopwatch stopwatch)
		{
			var builder = new StringBuilder();
			builder.AppendFormat("Found {0} newer plugin(s) (took {1}ms):", newerPlugins.Count,
				stopwatch.ElapsedMilliseconds);
			foreach (var plugin in plugins)
			{
				builder.AppendLine();

				var updatedPlugin = newerPlugins.FirstOrDefault(x => x.Id == plugin.Id);
				if (updatedPlugin != null)
					builder.AppendFormat("\t{0} => {1}", plugin, updatedPlugin);
				else
					builder.AppendFormat("\t{0}: No update found", plugin);
			}

			Log.Info(builder);
		}

		public IReadOnlyList<PluginIdentifier> FindAllPlugins()
		{
			var stopwatch = Stopwatch.StartNew();
			Log.DebugFormat("Retrieving all plugins...");

			var plugins = _plugins.GetAllKeys().ToList();

			stopwatch.Stop();
			Log.InfoFormat("Found {0} plugins (took {1}ms)", plugins.Count, stopwatch.ElapsedMilliseconds);

			return plugins;
		}

		public IReadOnlyList<PublishedPluginDescription> GetDescriptions(IReadOnlyList<PluginIdentifier> plugins)
		{
			var stopwatch = Stopwatch.StartNew();
			Log.DebugFormat("Retrieving description(s) for {0} plugin(s)...", plugins.Count);

			var descriptions = _pluginDescriptions.GetManyValues(plugins)
			                                      .Select(CreateDescription)
			                                      .ToList();

			stopwatch.Stop();
			Log.InfoFormat("Retrieved {0} description(s) from disk, (took {1}ms)",
			               descriptions.Count, stopwatch.ElapsedMilliseconds);

			return descriptions;
		}

		public IReadOnlyList<PublishedPluginDescription> GetAllPlugins()
		{
			var stopwatch = Stopwatch.StartNew();
			Log.DebugFormat("Retrieving description(s) for all plugin(s)...");

			var descriptions = _pluginDescriptions.GetAllValues()
				.Select(CreateDescription)
				.ToList();

			stopwatch.Stop();
			Log.InfoFormat("Retrieved {0} description(s) from disk, (took {1}ms)",
				descriptions.Count, stopwatch.ElapsedMilliseconds);

			return descriptions;
		}

		public IReadOnlyList<byte[]> GetIcons(IReadOnlyList<PluginIdentifier> plugins)
		{
			var stopwatch = Stopwatch.StartNew();
			Log.DebugFormat("Retrieving icons for {0} plugin(s)...", plugins.Count);

			var icons = _pluginIcons.GetManyValues(plugins).ToList();

			stopwatch.Stop();
			Log.InfoFormat("Retrieved {0} icon(s) from disk, (took {1}ms)",
			               icons.Count, stopwatch.ElapsedMilliseconds);

			return icons;
		}

		public byte[] DownloadPlugin(PluginIdentifier pluginId)
		{
			var stopwatch = Stopwatch.StartNew();
			Log.DebugFormat("Retrieving plugin '{0}'...", pluginId);

			try
			{
				var pluginContent = _plugins.Get(pluginId);

				stopwatch.Stop();
				Log.InfoFormat("Retrieved plugin '{0}' from disk, {1} bytes (took {2}ms)",
				               pluginId, pluginContent.Length, stopwatch.ElapsedMilliseconds);

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

		private bool IsValidEmail(string email)
		{
			try
			{
				// ReSharper disable once ObjectCreationAsStatement
				new MailAddress(email); //< Throws when the mail is invalid

				return true;
			}
			catch (Exception e)
			{
				Log.DebugFormat("Caught exception while parsing email '{0}':\r\n{1}", email, e);
				return false;
			}
		}

		public bool TryGetAccessToken(string username, out Guid accessToken)
		{
			if (!_users.TryGet(username, out var user))
			{
				accessToken = Guid.Empty;
				return false;
			}

			accessToken = user.AccessToken;
			return true;
		}

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

		private static DateTime GetBuildTime(Stream assembly)
		{
			using (var memoryStream = new MemoryStream(assembly.ReadToEnd()))
			{
				var header = PE.PeHeader.ReadFrom(memoryStream);
				return header.TimeStamp;
			}
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
					typeof(Change),
					typeof(PublishedPlugin)
				};
			}
		}

		[Pure]
		private static PublishedPluginDescription CreateDescription(PublishedPlugin plugin)
		{
			return new PublishedPluginDescription
			{
				Identifier = plugin.Identifier,
				Name = plugin.Name,
				Author = plugin.Author,
				Website = plugin.Website,
				Description = plugin.Description,
				Publisher = plugin.Publisher,
				PublishTimestamp = plugin.PublishDate
			};
		}
	}
}