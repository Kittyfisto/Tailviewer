using System;
using System.Text;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Settings;
using Tailviewer.Plugins;
using Tailviewer.Settings;

namespace Tailviewer.Test
{
	[TestFixture]
	public sealed class ServiceContainerTest
	{
		[Test]
		public void TestRetrieveRegisteredService()
		{
			var service = new Mock<ITypeFactory>().Object;

			var container = new ServiceContainer();
			container.RegisterInstance<ITypeFactory>(service);
			container.Retrieve<ITypeFactory>().Should().BeSameAs(service);
			container.TryRetrieve(out ITypeFactory service2).Should().BeTrue();
			service2.Should().BeSameAs(service);
		}

		[Test]
		public void TestRetrieveOverwrittenService()
		{
			var service1 = new Mock<ITypeFactory>().Object;
			var service2 = new Mock<ITypeFactory>().Object;

			var container = new ServiceContainer();
			container.RegisterInstance<ITypeFactory>(service1);
			container.RegisterInstance<ITypeFactory>(service2);

			var retrievedService = container.Retrieve<ITypeFactory>();
			retrievedService.Should().NotBeSameAs(service1);
			retrievedService.Should().BeSameAs(service2);
		}

		[Test]
		public void TestRetrieveUnregisteredService()
		{
			var container = new ServiceContainer();
			new Action(() => container.Retrieve<ITypeFactory>())
				.Should().Throw<ArgumentException>()
				.WithMessage("No service has been registered with this container which implements Tailviewer.ITypeFactory");

			container.TryRetrieve(out ITypeFactory service).Should().BeFalse();
			service.Should().BeNull();
		}

		[Test]
		[Description("Verifies that the encoding of the text log file can be specified via text log file settings")]
		public void TestCreateTextLogFileOverwriteEncoding()
		{
			var container = new ServiceContainer();
			container.RegisterInstance<ITaskScheduler>(new ManualTaskScheduler());
			container.RegisterInstance<ILogFileFormatMatcher>(new SimpleLogFileFormatMatcher(LogFileFormats.GenericText));
			container.RegisterInstance<ILogEntryParserPlugin>(new SimpleLogEntryParserPlugin());

			var settings = new LogFileSettings();
			settings.DefaultEncoding = Encoding.UTF32;
			container.RegisterInstance<ILogFileSettings>(settings);

			var logFile = container.CreateTextLogFile("foo");
			logFile.GetProperty(GeneralProperties.Encoding).Should().Be(Encoding.UTF32);
		}
	}
}
