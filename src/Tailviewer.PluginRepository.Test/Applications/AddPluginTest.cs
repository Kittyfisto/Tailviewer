using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.PluginRepository.Applications;

namespace Tailviewer.PluginRepository.Test.Applications
{
	[TestFixture]
	public sealed class AddPluginTest
	{
		[Test]
		public void TestAddPluginNoSuchDirectory()
		{
			var app = new AddPlugin();

			var filesystem = new InMemoryFilesystem();
			var repo = new Mock<IInternalPluginRepository>();
			app.Run(filesystem, repo.Object, new AddPluginOptions
			{
				PluginFileName = @"M:\does\not\exist.tvp"
			}).Should().Be(-10);

			repo.Verify(x => x.PublishPlugin(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never);
			repo.Verify(x => x.PublishPlugin(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		[Test]
		public void TestAddPluginNoSuchFile()
		{
			var filesystem = new InMemoryFilesystem();
			var path = @"M:\does\not\exist\";
			filesystem.CreateDirectory(path);

			var repo = new Mock<IInternalPluginRepository>();
			var file = Path.Combine(path, "plugin.tvp");

			var app = new AddPlugin();
			app.Run(filesystem, repo.Object, new AddPluginOptions
			{
				PluginFileName = file
			}).Should().Be(-10);

			repo.Verify(x => x.PublishPlugin(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never);
			repo.Verify(x => x.PublishPlugin(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}
	}
}
