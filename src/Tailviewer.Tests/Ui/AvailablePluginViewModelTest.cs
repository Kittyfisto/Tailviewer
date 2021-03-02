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
		public void TestConstruction()
		{
			var plugin = new PublishedPluginDescription
			{
				Name = "Foo",
				Identifier = new PluginIdentifier("TestPlugin", Version.Parse("1.4.3"))
			};
			var viewModel = new AvailablePluginViewModel(plugin, () => {});
			viewModel.Website.Should().BeNull();
		}
	}
}
