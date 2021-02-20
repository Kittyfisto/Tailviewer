using System;
using System.IO;
using System.Text;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Core;
using Tailviewer.Core.Sources.Text;
using Tailviewer.Plugins;
using Tailviewer.Test;

namespace Tailviewer.AcceptanceTests.BusinessLogic.Sources.Text
{
	[TestFixture]
	public sealed class StreamingTextLogFileAcceptanceTest
	{
		private ServiceContainer _serviceContainer;
		private DefaultTaskScheduler _taskScheduler;
		private SimpleLogFileFormatMatcher _formatMatcher;
		private SimpleLogEntryParserPlugin _logEntryParserPlugin;

		[SetUp]
		public void SetUp()
		{
			_serviceContainer = new ServiceContainer();
			_taskScheduler = new DefaultTaskScheduler();
			_formatMatcher = new SimpleLogFileFormatMatcher(LogFileFormats.GenericText);
			_logEntryParserPlugin = new SimpleLogEntryParserPlugin();

			_serviceContainer.RegisterInstance<ITaskScheduler>(_taskScheduler);
			_serviceContainer.RegisterInstance<ILogFileFormatMatcher>(_formatMatcher);
			_serviceContainer.RegisterInstance<ILogEntryParserPlugin>(_logEntryParserPlugin);
		}

		private StreamingTextLogSource Create(string fileName, Encoding encoding)
		{
			return new StreamingTextLogSource(_taskScheduler, fileName, encoding);
		}

		private string GetUniqueNonExistingFileName()
		{
			var fileName = PathEx.GetTempFileName();
			if (File.Exists(fileName))
				File.Delete(fileName);

			TestContext.WriteLine("FileName: {0}", fileName);
			return fileName;
		}

		[Test]
		public void TestNotifyListenersEventually()
		{
			var line = "Hello, World!";
			var fileName = GetUniqueNonExistingFileName();

			using (var handle = new ManualResetEvent(false))
			{
				var logFile = Create(fileName, Encoding.Default);
				var listener = new Mock<ILogSourceListener>();
				listener.Setup(x => x.OnLogFileModified(logFile, new LogFileSection(0, 1)))
				        .Callback(() => handle.Set());
				logFile.AddListener(listener.Object, TimeSpan.FromMilliseconds(100), 2);

				using (var stream = File.OpenWrite(fileName))
				using (var writer = new StreamWriter(stream, Encoding.Default))
				{
					writer.Write(line);
				}

				handle.WaitOne(TimeSpan.FromSeconds(2))
				      .Should().BeTrue("because the listener should eventually (after ~100ms) be notified that one line was added");
			}
		}
	}
}