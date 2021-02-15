using System;
using System.Text;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core;
using Tailviewer.Core.LogFiles;
using Tailviewer.Core.LogFiles.Text;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles.Text
{
	[TestFixture]
	public sealed class FileLogFileTest
	{
		private ServiceContainer _services;
		private ManualTaskScheduler _taskScheduler;
		private Mock<ITextLogFileParserPlugin2> _parser;
		private Mock<ILogFileFormatMatcher> _formatMatcher;
		private Mock<ILogFile> _textLogSource;

		[SetUp]
		public void Setup()
		{
			_services = new ServiceContainer();
			_taskScheduler = new ManualTaskScheduler();
			_parser = new Mock<ITextLogFileParserPlugin2>();
			_textLogSource = new Mock<ILogFile>();
			_parser.Setup(x => x.CreateParser(It.IsAny<IServiceContainer>(), It.IsAny<ILogFile>()))
			       .Returns(_textLogSource.Object);
			_formatMatcher = new Mock<ILogFileFormatMatcher>();

			_services.RegisterInstance<ITaskScheduler>(_taskScheduler);
			_services.RegisterInstance<ITextLogFileParserPlugin2>(_parser.Object);
			_services.RegisterInstance<ILogFileFormatMatcher>(_formatMatcher.Object);
		}

		private FileLogFile Create(string fileName)
		{
			return new FileLogFile(_services, fileName, TimeSpan.Zero);
		}

		[Test]
		public void TestEncoding_Detect_Utf8_Bom()
		{
			using (var logFile = Create(@"TestData\Encodings\utf8_w_bom.txt"))
			{
				logFile.GetProperty(Properties.Encoding).Should().BeNull();

				_taskScheduler.RunOnce();
				logFile.GetProperty(Properties.Encoding).Should().Be(Encoding.UTF8);
			}
		}
	}
}
