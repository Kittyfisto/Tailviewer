using System.Text.RegularExpressions;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class PluginLogFileFactoryTest
	{
		private ManualTaskScheduler _scheduler;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_scheduler = new ManualTaskScheduler();
		}

		[Test]
		public void TestOpen1()
		{
			// How should tailviewer as a whole behave when a plugin says it's responsible for extension
			// x but then it crashes during Open? Should be just hide this crash from the user and
			// display it using TextLogFile instead? Or do we display an error to the user?
			// Depending on which way we decide, the current implementation (and this test)
			// need to be changed...
		}

		[Test]
		public void TestOpen2()
		{
			var fname = @"D:\some awesome log file.db";
			var plugin = new Mock<IFileFormatPlugin>();
			var logFile = new Mock<ILogFile>();
			plugin.Setup(x => x.SupportedExtensions).Returns(new[] {".db"});
			plugin.Setup(x => x.Open(It.Is<string>(y => y == fname), It.IsAny<ITaskScheduler>()))
				.Returns(() => logFile.Object);

			var factory = new PluginLogFileFactory(_scheduler, plugin.Object);
			var actualLogFile = factory.Open(fname, out _);
			actualLogFile.Should().BeOfType<NoThrowLogFile>("because PluginLogFileFactory should protect us from buggy plugin implementations");

			actualLogFile.GetLine(42);
			logFile.Verify(x => x.GetLine(It.Is<int>(y => y == 42)), Times.Once,
				"because even though we've been given a proxy, it should nevertheless forward all calls to the actual implementation");
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/189")]
		[Description("Verifies that a plugin will be used if it matches a filename via regular expression")]
		public void TestOpenMatchRegex()
		{
			var plugin = new Mock<IFileFormatPlugin2>();
			var logFile = new Mock<ILogFile>();
			plugin.Setup(x => x.SupportedFileNames).Returns(new[]
			{
				new Regex("txt"),
				new Regex("sfa_[^\\.]*\\.log")
			});
			plugin.Setup(x => x.SupportedExtensions).Returns(new[] {".db"});
			plugin.Setup(x => x.Open(It.IsAny<string>(), It.IsAny<ITaskScheduler>()))
			      .Returns(() => logFile.Object);

			var factory = new PluginLogFileFactory(_scheduler, plugin.Object);
			var filename = @"C:\logs\sfa_12345.log";
			var actualLogFile = factory.Open(filename, out _);

			plugin.Verify(x => x.Open(filename, It.IsAny<ITaskScheduler>()), Times.Once);
			actualLogFile.GetLine(42);
			logFile.Verify(x => x.GetLine(It.Is<int>(y => y == 42)), Times.Once,
			               "because even though we've been given a proxy, it should nevertheless forward all calls to the actual implementation");
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/189")]
		[Description("Verifies that a plugin will NOT be used if neither its regex nor file extesion matches")]
		public void TestOpenDontMatchRegex()
		{
			var plugin = new Mock<IFileFormatPlugin2>();
			var logFile = new Mock<ILogFile>();
			plugin.Setup(x => x.SupportedFileNames).Returns(new[]
			{
				new Regex("txt"),
				new Regex("sfa_[^\\.]*\\.log")
			});
			plugin.Setup(x => x.SupportedExtensions).Returns(new[] {".db"});
			plugin.Setup(x => x.Open(It.IsAny<string>(), It.IsAny<ITaskScheduler>()))
			      .Returns(() => logFile.Object);

			var factory = new PluginLogFileFactory(_scheduler, plugin.Object);
			var filename = @"C:\sfa_12345.log\.db\txt\foo.log";
			factory.Open(filename, out _);
			plugin.Verify(x => x.Open(filename, It.IsAny<ITaskScheduler>()), Times.Never,
			              "because neither regex nor file extension match (only the filename should be matched, not the path) and therefore the plugin may not have been used");
		}
	}
}