using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Repository;
using Tailviewer.Ui.Plugins;

namespace Tailviewer.Tests.Ui
{
	[TestFixture]
	public sealed class AvailablePluginViewModelTest
	{
		[Test]
		public void TestConstructionNull()
		{
			var plugin = new PublishedPluginDescription
			{
				Name = "Foo",
				Identifier = new PluginIdentifier("TestPlugin", Version.Parse("1.4.3")),
				Website = null
			};
			var viewModel = new AvailablePluginViewModel(plugin, () => {});
			viewModel.Website.Should().BeNull();
		}

		[Test]
		public void TestConstructionEmptyWebsite()
		{
			var plugin = new PublishedPluginDescription
			{
				Name = "Foo",
				Identifier = new PluginIdentifier("TestPlugin", Version.Parse("1.4.3")),
				Website = ""
			};
			var viewModel = new AvailablePluginViewModel(plugin, () => {});
			viewModel.Website.Should().BeNull();
		}

		[Test]
		public void TestConstructionInvalidWebsite()
		{
			var plugin = new PublishedPluginDescription
			{
				Name = "Foo",
				Identifier = new PluginIdentifier("TestPlugin", Version.Parse("1.4.3")),
				Website = "Fwaadasw"
			};
			var viewModel = new AvailablePluginViewModel(plugin, () => {});
			viewModel.Website.Should().BeNull();
		}
	}
}
