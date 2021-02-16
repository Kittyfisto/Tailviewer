using System;
using System.Text;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources.Text;
using Tailviewer.Plugins;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles.Text
{
	[TestFixture]
	public sealed class FileLogFileTest
	{
		private ServiceContainer _services;
		private ManualTaskScheduler _taskScheduler;
		private Mock<ITextLogFileParserPlugin2> _parser;
		private Mock<ILogFileFormatMatcher> _formatMatcher;
		private Mock<ILogSource> _textLogSource;

		[SetUp]
		public void Setup()
		{
			_services = new ServiceContainer();
			_taskScheduler = new ManualTaskScheduler();
			_parser = new Mock<ITextLogFileParserPlugin2>();
			_textLogSource = new Mock<ILogSource>();
			_parser.Setup(x => x.CreateParser(It.IsAny<IServiceContainer>(), It.IsAny<ILogSource>()))
			       .Returns(_textLogSource.Object);
			_formatMatcher = new Mock<ILogFileFormatMatcher>();

			_services.RegisterInstance<ITaskScheduler>(_taskScheduler);
			_services.RegisterInstance<ITextLogFileParserPlugin2>(_parser.Object);
			_services.RegisterInstance<ILogFileFormatMatcher>(_formatMatcher.Object);
		}

		private SourceLogSource Create(string fileName)
		{
			return new SourceLogSource(_services, fileName, TimeSpan.Zero);
		}

		[Test]
		public void TestEncoding_Detect_Utf8_Bom()
		{
			using (var logFile = Create(@"TestData\Encodings\utf8_w_bom.txt"))
			{
				logFile.GetProperty(GeneralProperties.Encoding).Should().BeNull();

				_taskScheduler.RunOnce();
				logFile.GetProperty(GeneralProperties.Encoding).Should().Be(Encoding.UTF8);
			}
		}
	}
}
