using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Repository;
using Tailviewer.PluginRepository.Applications;
using Tailviewer.PluginRepository.Configuration;

namespace Tailviewer.PluginRepository.Test
{
	[TestFixture]
	public sealed class PluginRepositoryProxyTest
	{
		[Test]
		public void TestProhibitPublishing()
		{
			var mock = new Mock<IPluginRepository>();
			var configuration = new ServerConfiguration
			{
				Publishing = {AllowRemotePublish = false}
			};
			var proxy = new PluginRepositoryProxy(mock.Object, configuration);
			new Action(() => proxy.PublishPlugin(new byte[1234], "a"))
				.Should()
				.Throw<RemotePublishDisabledException>();
			mock.Verify(x => x.PublishPlugin(It.IsAny<byte[]>(), It.IsAny<string>()),
			            Times.Never);
		}
	}
}
