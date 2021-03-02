using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core;
using Tailviewer.Core.Sources.Text;

namespace Tailviewer.Tests.BusinessLogic.Sources
{
	[TestFixture]
	public sealed class ParsingLogSourceFactoryTest
	{
		private ServiceContainer _services;
		private PluginRegistry _pluginRegistry;

		[SetUp]
		public void Setup()
		{
			_services = new ServiceContainer();
			_pluginRegistry = new PluginRegistry();
			_services.RegisterInstance<IPluginLoader>(_pluginRegistry);
		}

		private ParsingLogSourceFactory CreateFactory()
		{
			return new ParsingLogSourceFactory(_services);
		}

		private ILogSource CreateSource(ILogFileFormat format)
		{
			var source = new Mock<ILogSource>();
			source.Setup(x => x.GetProperty(GeneralProperties.Format)).Returns(format);
			source.Setup(x => x.Columns).Returns(new IColumnDescriptor[0]);
			return source.Object;
		}

		[Test]
		public void TestCreateParser_No_Plugins()
		{
			var factory = CreateFactory();
			var parser = factory.CreateParser(null, CreateSource(LogFileFormats.GenericText));
			parser.Should().BeOfType<GenericTextLogSource>();
		}

		[Test]
		public void TestCreateParser_LogSourceParserPlugin_Throws()
		{
			var factory = CreateFactory();

			var plugin = new Mock<ILogSourceParserPlugin>();
			plugin.Setup(x => x.CreateParser(It.IsAny<IServiceContainer>(), It.IsAny<ILogSource>()))
			      .Throws<ArgumentException>();
			_pluginRegistry.Register(plugin.Object);

			var parser = factory.CreateParser(null, CreateSource(LogFileFormats.GenericText));
			parser.Should().BeOfType<GenericTextLogSource>();
		}

		[Test]
		public void TestCreateParser_LogEntryParserPlugin_Throws()
		{
			var factory = CreateFactory();

			var plugin = new Mock<ILogEntryParserPlugin>();
			plugin.Setup(x => x.CreateParser(It.IsAny<IServiceContainer>(), It.IsAny<ILogFileFormat>()))
			      .Throws<ArgumentException>();
			_pluginRegistry.Register(plugin.Object);

			var parser = factory.CreateParser(null, CreateSource(LogFileFormats.GenericText));
			parser.Should().BeOfType<GenericTextLogSource>();
		}
	}
}
