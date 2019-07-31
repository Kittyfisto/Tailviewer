using System;
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
		private IDatabase _database;
		private IDictionary<string, User> _users;
		private IDictionary<Guid, string> _usernamesByAccessToken;
		private IDictionary<PluginIdentifier, byte[]> _plugins;
		private IDictionary<PluginIdentifier, PublishedPlugin> _pluginRequirements;
		private IDictionary<PluginIdentifier, byte[]> _pluginIcons;

		[SetUp]
		public void Setup()
		{
			_database = Database.CreateInMemory(PluginRepository.CustomTypes);
			_repository = new PluginRepository(_database, newlyCreated: false);

			_users = _database.GetDictionary<string, User>("Users");
			_usernamesByAccessToken = _database.GetDictionary<Guid, string>("UsersByAccessToken");
			_plugins = _database.GetDictionary<PluginIdentifier, byte[]>("Plugins");
			_pluginIcons = _database.GetDictionary<PluginIdentifier, byte[]>("PluginIcons");
			_pluginRequirements = _database.GetDictionary<PluginIdentifier, PublishedPlugin>("PluginDescriptions");
		}

		[TearDown]
		public void TearDown()
		{
			_repository?.Dispose();
		}

		[Test]
		public void TestAddUserInvalidName([Values(null, "", "a b c", "씨엘씨", "micke~4213")] string username)
		{
			new Action(() => _repository.AddUser(username, "a@b.c", null))
				.Should()
				.Throw<CannotAddUserException>();

			DatabaseShouldBeEmpty();
		}

		[Test]
		public void TestAddUserInvalidEmail([Values(null, "", "a", "foobar", "씨엘씨", "micky@sutro8)")] string email)
		{
			new Action(() => _repository.AddUser("mickey", email, null))
				.Should()
				.Throw<CannotAddUserException>();

			DatabaseShouldBeEmpty();
		}

		private void DatabaseShouldBeEmpty()
		{
			_users.GetAll().Should().BeEmpty();
			_usernamesByAccessToken.GetAll().Should().BeEmpty();
			_pluginRequirements.GetAll().Should().BeEmpty();
			_pluginIcons.GetAll().Should().BeEmpty();
			_plugins.GetAll().Should().BeEmpty();
		}
	}
}