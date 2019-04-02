using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.Ui.Controls.MainPanel.Plugins;

namespace Tailviewer.Test.Ui.Controls.MainPanel.Plugins
{
	[TestFixture]
	public sealed class PluginViewModelTest
	{
		[Test]
		public void TestConstructionWithoutName([Values(null, "")] string emptyName)
		{
			var description = new PluginDescription
			{
				Id = new PluginId("SomeOrg.SomePlugin"),
				Name = emptyName
			};
			var viewModel = new PluginViewModel(description);
			viewModel.Name.Should().Be("SomeOrg.SomePlugin",
				"because absent of a name, the id should be presented to the user as the plugin's name");
		}

		[Test]
		public void TestConstructionWithError()
		{
			var description = new PluginDescription
			{
				Error = "Someone screwed up"
			};
			var viewModel = new PluginViewModel(description);
			viewModel.HasError.Should().BeTrue();
			viewModel.Error.Should().Be("Someone screwed up");
		}
	}
}
