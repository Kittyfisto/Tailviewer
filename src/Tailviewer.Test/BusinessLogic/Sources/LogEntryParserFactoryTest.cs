using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core;
using Tailviewer.Plugins;

namespace Tailviewer.Test.BusinessLogic.Sources
{
	[TestFixture]
	public sealed class LogEntryParserFactoryTest
	{
		[Test]
		public void TestNoPlugin()
		{
			var services = new ServiceContainer();
			var plugins = new PluginRegistry();
			services.RegisterInstance<IPluginLoader>(plugins);
			var factory = new LogEntryParserFactory(services);

			factory.CreateParser(services, LogFileFormats.GenericText)
			       .Should().BeOfType<GenericTextLogEntryParser>("because there's no plugin registered at all");
		}

		[Test]
		public void TestPluginThrows()
		{
			var services = new ServiceContainer();
			var plugins = new PluginRegistry();
			var plugin = new Mock<ILogEntryParserPlugin>();
			plugin.Setup(x => x.CreateParser(services, LogFileFormats.GenericText)).Throws<NullReferenceException>();
			plugins.Register(plugin.Object);
			services.RegisterInstance<IPluginLoader>(plugins);
			var factory = new LogEntryParserFactory(services);

			factory.CreateParser(services, LogFileFormats.GenericText)
			       .Should().BeOfType<GenericTextLogEntryParser>("because the only plugin registered crashes");

			plugin.Verify(x => x.CreateParser(services, LogFileFormats.GenericText), Times.Once);
		}
	}
}
