using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core;
using Tailviewer.Core.Sources;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources.Text;
using Tailviewer.Formats.Serilog;
using Tailviewer.Plugins;

namespace Tailviewer.Serilog.Test
{
	[TestFixture]
	public sealed class SerilogAcceptanceTest
	{
		private DefaultTaskScheduler _taskScheduler;
		private IServiceContainer _services;
		private Mock<ILogFileFormatRepository> _repository;
		private List<SerilogFileFormat> _formats;
		private PluginRegistry _plugins;
		private PluginLogFileFactory _logSourceFactory;

		[SetUp]
		public void Setup()
		{
			_taskScheduler = new DefaultTaskScheduler();
			_formats = new List<SerilogFileFormat>();
			_repository = new Mock<ILogFileFormatRepository>();
			_repository.Setup(x => x.Formats).Returns(() => _formats);
			_plugins = new PluginRegistry();
			_plugins.Register(new SerilogEntryParserPlugin());

			_services = new ServiceContainer();
			_services.RegisterInstance<ITaskScheduler>(_taskScheduler);
			_services.RegisterInstance<ILogFileFormatMatcher>(new SerilogFileFormatMatcher(_repository.Object));
			_services.RegisterInstance<IRawFileLogSourceFactory>(new RawFileLogSourceFactory(_taskScheduler));
			_services.RegisterInstance<IPluginLoader>(_plugins);
			_services.RegisterInstance<ILogSourceParserPlugin>(new ParsingLogSourceFactory(_services));
			_logSourceFactory = new PluginLogFileFactory(_services, null);
		}

		[TearDown]
		public void TearDown()
		{
			_taskScheduler.Dispose();
		}

		[Test]
		public void Test()
		{
			_formats.Add(new SerilogFileFormat("sss", "{Timestamp:yyyy-MM-dd HH:mm:ss.fff K} [{Level:u3}] {Message}", Encoding.Default));

			using (var logSource = _logSourceFactory.Open(@"TestData\Formats\Serilog\Serilog.txt"))
			{
				logSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);
				logSource.GetProperty(GeneralProperties.EmptyReason).Should().Be(ErrorFlags.None);
				logSource.GetProperty(GeneralProperties.LogEntryCount).Should().BeGreaterOrEqualTo(11);

				var entries = logSource.GetEntries();
				entries[0].Index.Should().Be(0);
				entries[0].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 00, 12, 207));
				entries[0].LogLevel.Should().Be(LevelFlags.Debug);
				entries[0].Message.Should().Be("Fetch modification job triggered at 9/13/2020 12:00:12 AM!!!");

				entries[1].Index.Should().Be(1);
				entries[1].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 00, 13, 46));
				entries[1].LogLevel.Should().Be(LevelFlags.Info);
				entries[1].Message.Should().Be("1 modified account received for date 1399/06/23");

				entries[2].Index.Should().Be(2);
				entries[2].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 00, 28, 524));
				entries[2].LogLevel.Should().Be(LevelFlags.Trace);
				entries[2].Message.Should().Be("Personage modification job triggered at 9/13/2020 12:00:28 AM!!!");

				entries[3].Index.Should().Be(3);
				entries[3].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 00, 28, 527));
				entries[3].LogLevel.Should().Be(LevelFlags.Info);
				entries[3].Message.Should().Be("1 items are in queue to process");

				entries[4].Index.Should().Be(4);
				entries[4].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 00, 34, 402));
				entries[4].LogLevel.Should().Be(LevelFlags.Info);
				entries[4].Message.Should().Be("User with NationalCode YXZ-OPRRTTT-1 was not in system and account hasbeen created with personage Id 603860");

				entries[5].Index.Should().Be(5);
				entries[5].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 00, 37, 300));
				entries[5].LogLevel.Should().Be(LevelFlags.Info);
				entries[5].Message.Should().Be("User created for Personage Id 603860 and sms has been sent to 0910XXXXXXX");

				entries[6].Index.Should().Be(6);
				entries[6].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 00, 38, 398));
				entries[6].LogLevel.Should().Be(LevelFlags.Warning);
				entries[6].Message.Should().Be("Customer Modified in personage service with NationalCode YXZ-OPRRTTT-1 and PersonageId 603860");

				entries[7].Index.Should().Be(7);
				entries[7].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 01, 08, 396));
				entries[7].Message.Should().Be("Personage modification job triggered at 9/13/2020 12:01:08 AM!!!");
				entries[7].LogLevel.Should().Be(LevelFlags.Info);

				entries[8].Index.Should().Be(8);
				entries[8].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 01, 08, 399));
				entries[8].LogLevel.Should().Be(LevelFlags.Error);
				entries[8].Message.Should().Be("0 items are in queue to process");

				entries[9].Index.Should().Be(9);
				entries[9].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 01, 38, 402));
				entries[9].LogLevel.Should().Be(LevelFlags.Info);
				entries[9].Message.Should().Be("Personage modification job triggered at 9/13/2020 12:01:38 AM!!!");

				entries[10].Index.Should().Be(10);
				entries[10].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 01, 38, 405));
				entries[10].LogLevel.Should().Be(LevelFlags.Fatal);
				entries[10].Message.Should().Be("0 items are in queue to process");
			}
		}
	}
}
