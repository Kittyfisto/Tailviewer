using System;
using System.IO;
using FluentAssertions;
using IsabelDb;
using NUnit.Framework;
using Tailviewer.Archiver.Repository;
using Tailviewer.PluginRepository.Entities;
using Tailviewer.PluginRepository.Exceptions;

namespace Tailviewer.PluginRepository.Test
{
	[TestFixture]
	public class PluginRepositoryTest
	{
		private PluginRepository _repository;
		private InMemoryFilesystem _filesystem;
		private IDatabase _database;
		private IDictionary<string, User> _users;
		private IDictionary<Guid, string> _usernamesByAccessToken;
		private IDictionary<PluginIdentifier, byte[]> _pluginsById;
		private IDictionary<PluginIdentifier, PublishedPlugin> _pluginRequirements;

		[SetUp]
		public void Setup()
		{
			_filesystem = new InMemoryFilesystem();
			_database = Database.CreateInMemory(PluginRepository.CustomTypes);
			_repository = new PluginRepository(_filesystem, _database);

			_users = _database.GetOrCreateDictionary<string, User>("Users");
			_usernamesByAccessToken = _database.GetOrCreateDictionary<Guid, string>("UsersByAccessToken");
			_pluginsById = _database.GetOrCreateDictionary<PluginIdentifier, byte[]>("PluginsById");
			_pluginRequirements = _database.GetOrCreateDictionary<PluginIdentifier, PublishedPlugin>("PluginRequirements");
		}

		[TearDown]
		public void TearDown()
		{
			_repository?.Dispose();
		}

		[Test]
		public void TestAddPluginNoSuchDirectory()
		{
			new Action(() => _repository.AddPlugin(@"M:\does\not\exist.tvp", ""))
				.Should()
				.Throw<CannotAddPluginException>()
				.WithInnerException<DirectoryNotFoundException>();

			DatabaseShouldBeEmpty();
		}

		private void DatabaseShouldBeEmpty()
		{
			_users.GetAll().Should().BeEmpty();
			_usernamesByAccessToken.GetAll().Should().BeEmpty();
			_pluginRequirements.GetAll().Should().BeEmpty();
			_pluginsById.GetAll().Should().BeEmpty();
		}
	}
}