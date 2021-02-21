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
			_services.RegisterInstance<IFileLogSourceFactory>(new FileLogSourceFactory(_taskScheduler));
			_services.RegisterInstance<IPluginLoader>(_plugins);
			_services.RegisterInstance<ILogSourceParserPlugin>(new ParsingLogSourceFactory(_services));
		}

		[TearDown]
		public void TearDown()
		{
			_taskScheduler.Dispose();
		}

		private ILogSource Create(string fileName)
		{
			return _services.CreateTextLogFile(fileName);
		}

		[Test]
		public void Test()
		{
			_formats.Add(new SerilogFileFormat("sss", "{Timestamp:yyyy-MM-dd HH:mm:ss.fff K} [{Level:u3}] {Message}", Encoding.Default));

			using (var logSource = Create(@"TestData\Formats\Serilog\Serilog.txt"))
			{
				logSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);
				logSource.GetProperty(GeneralProperties.EmptyReason).Should().Be(ErrorFlags.None);
				logSource.GetProperty(GeneralProperties.LogEntryCount).Should().BeGreaterOrEqualTo(11);

				var entries = logSource.GetEntries();
				entries[0].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 00, 12, 207));
				entries[0].LogLevel.Should().Be(LevelFlags.Debug);
				
				entries[1].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 00, 13, 46));
				entries[1].LogLevel.Should().Be(LevelFlags.Info);
				
				entries[2].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 00, 28, 524));
				entries[2].LogLevel.Should().Be(LevelFlags.Trace);
				
				entries[3].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 00, 28, 527));
				entries[3].LogLevel.Should().Be(LevelFlags.Info);
				
				entries[4].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 00, 34, 402));
				entries[4].LogLevel.Should().Be(LevelFlags.Info);
				
				entries[5].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 00, 37, 300));
				entries[5].LogLevel.Should().Be(LevelFlags.Info);
				
				entries[6].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 00, 38, 398));
				entries[6].LogLevel.Should().Be(LevelFlags.Warning);
				
				entries[7].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 01, 08, 396));
				entries[7].LogLevel.Should().Be(LevelFlags.Info);
				
				entries[8].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 01, 08, 399));
				entries[8].LogLevel.Should().Be(LevelFlags.Error);
				
				entries[9].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 01, 38, 402));
				entries[9].LogLevel.Should().Be(LevelFlags.Info);
				
				entries[10].Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 01, 38, 405));
				entries[10].LogLevel.Should().Be(LevelFlags.Fatal);
			}
		}
	}
}
