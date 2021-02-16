using System;
using System.IO;
using System.Text;
using System.Threading;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources;
using Tailviewer.Core.Sources.Text;
using Tailviewer.Plugins;
using Tailviewer.Test;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles.Text
{
	[TestFixture]
	public sealed class FileLogSourceAcceptanceTest
	{
		private ServiceContainer _services;
		private DefaultTaskScheduler _taskScheduler;
		private Mock<ILogSourceParserPlugin> _parser;
		private SimpleLogFileFormatMatcher _formatMatcher;

		[SetUp]
		public void Setup()
		{
			_services = new ServiceContainer();
			_taskScheduler = new DefaultTaskScheduler();
			_parser = new Mock<ILogSourceParserPlugin>();
			_parser.Setup(x => x.CreateParser(It.IsAny<IServiceContainer>(), It.IsAny<ILogSource>()))
			       .Returns((IServiceContainer services, ILogSource source) =>
			       {
				       return new GenericTextLogSource(services, source, new GenericTextLogEntryParser());
			       });

			_formatMatcher = new SimpleLogFileFormatMatcher(null);

			_services.RegisterInstance<ITaskScheduler>(_taskScheduler);
			_services.RegisterInstance<ILogSourceParserPlugin>(_parser.Object);
			_services.RegisterInstance<ILogFileFormatMatcher>(_formatMatcher);
		}

		private FileLogSource Create(string fileName)
		{
			return new FileLogSource(_services, fileName, TimeSpan.Zero);
		}

		[Test]
		public void TestEncoding_Detect_Utf8_Bom()
		{
			var fileName = @"TestData\Encodings\utf8_w_bom.txt";
			using (var logSource = Create(fileName))
			{
				logSource.GetProperty(GeneralProperties.Encoding).Should().BeNull();
				logSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);
				//logFile.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromDays(1)).Be(Percentage.HundredPercent);

				var info = new FileInfo(fileName);
				logSource.GetProperty(GeneralProperties.Encoding).Should().Be(Encoding.UTF8, "because the source file has a BOM which should have been detected");
				logSource.GetProperty(GeneralProperties.Format).Should().Be(LogFileFormats.GenericText, "because we didn't provide the log file with a working detector");
				logSource.GetProperty(GeneralProperties.Created).Should().Be(info.CreationTimeUtc);
				logSource.GetProperty(GeneralProperties.LastModified).Should().Be(info.LastWriteTimeUtc);
				logSource.GetProperty(GeneralProperties.Size).Should().Be(Size.FromBytes(info.Length));

				var entries = logSource.GetEntries();
				entries.Should().HaveCount(1);
				entries[0].RawContent.Should().Be("2015-10-07 19:50:58,997 [8092, 1] DEBUG SharpRemote.Hosting.OutOfProcessSiloServer (null) - Args.Length: 1");
			}
		}
	}
}
