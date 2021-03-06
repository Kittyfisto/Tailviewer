﻿using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins;

namespace Tailviewer.Archiver.Tests
{
	[TestFixture]
	public sealed class PluginRegistryTest
	{
		[Test]
		public void TestPlugins()
		{
			var registry = new PluginRegistry();
			registry.Plugins.Should().NotBeNull();
			registry.Plugins.Should().BeEmpty();
		}

		[Test]
		public void TestLoadEmptyRegistry()
		{
			var registry = new PluginRegistry();
			registry.LoadAllOfType<ILogFileOutlinePlugin>().Should().BeEmpty();
			registry.LoadAllOfType<ILogEntryParserPlugin>().Should().BeEmpty();
		}

		[Test]
		public void TestLoadRegisteredPlugin()
		{
			var registry = new PluginRegistry();

			var plugin = CreateOutlinePlugin();
			registry.Register(plugin);

			var plugins = registry.LoadAllOfType<ILogFileOutlinePlugin>();
			plugins.Should().NotBeNull();
			plugins.Should().HaveCount(1);
			plugins.First().Should().BeSameAs(plugin);

			var plugins2 = registry.LoadAllOfTypeWithDescription<ILogFileOutlinePlugin>();
			plugins2.Should().NotBeNull();
			plugins2.Should().HaveCount(1);
			plugins2[0].Plugin.Should().BeSameAs(plugin);
			plugins2[0].Description.Should().BeNull();
		}

		private ILogFileOutlinePlugin CreateOutlinePlugin()
		{
			var plugin = new Mock<ILogFileOutlinePlugin>();
			return plugin.Object;
		}
	}
}
