using System.IO;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core;
using Tailviewer.Core.LogFiles;
using Tailviewer.Core.LogFiles.Text;
using Tailviewer.Test;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles.Text
{
	[TestFixture]
	public abstract class AbstractStreamingTextLogFileTest
	{
		private ServiceContainer _serviceContainer;
		private ManualTaskScheduler _taskScheduler;
		private string _fileName;
		private SimpleLogFileFormatMatcher _formatMatcher;
		private SimpleTextLogFileParserPlugin _textLogFileParserPlugin;

		[SetUp]
		public void SetUp()
		{
			_serviceContainer = new ServiceContainer();
			_taskScheduler = new ManualTaskScheduler();
			_formatMatcher = new SimpleLogFileFormatMatcher(LogFileFormats.GenericText);
			_fileName = PathEx.GetTempFileName();
			_textLogFileParserPlugin = new SimpleTextLogFileParserPlugin();

			_serviceContainer.RegisterInstance<ITaskScheduler>(_taskScheduler);
			_serviceContainer.RegisterInstance<ILogFileFormatMatcher>(_formatMatcher);
			_serviceContainer.RegisterInstance<ITextLogFileParserPlugin>(_textLogFileParserPlugin);

			TestContext.WriteLine("FileName: {0}", _fileName);
		}

		private StreamingTextLogFile Create()
		{
			return Create(_serviceContainer, _fileName);
		}

		internal abstract StreamingTextLogFile Create(ServiceContainer serviceContainer, string fileName);

		[Test]
		public void TestFileDoesNotExist()
		{
			var logFile = Create();
			logFile.Count.Should().Be(0);
			logFile.OriginalCount.Should().Be(0);
			logFile.GetValue(LogFileProperties.Size).Should().BeNull("because the log file didn't even have enough time to check the source");
			logFile.GetValue(LogFileProperties.Created).Should().BeNull("because the log file didn't even have enough time to check the source");
			logFile.GetValue(LogFileProperties.PercentageProcessed).Should().Be(Percentage.Zero);
			logFile.GetValue(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.None, "because the log file didn't have enough time to check the source");

			_taskScheduler.RunOnce();

			logFile.Count.Should().Be(0);
			logFile.GetValue(LogFileProperties.Size).Should().BeNull("because the source file does not exist");
			logFile.GetValue(LogFileProperties.Created).Should().BeNull("because the source file does not exist");
			logFile.GetValue(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.SourceDoesNotExist, "because the source file does not exist");
			logFile.GetValue(LogFileProperties.PercentageProcessed).Should().Be(Percentage.HundredPercent, "because we've checked that the source doesn't exist and thus there's nothing more to process");
		}
	}
}
