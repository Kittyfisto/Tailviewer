using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core.Sources;

namespace Tailviewer.Acceptance.Tests.BusinessLogic.Sources
{
	[TestFixture]
	public sealed class LogSourceProxyTest
	{
		private DefaultTaskScheduler _taskScheduler;
		private Mock<ILogSource> _logFile;
		private LogSourceListenerCollection _listeners;
		private Mock<ILogSourceListener> _listener;
		private List<LogSourceModification> _modifications;

		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			_taskScheduler = new DefaultTaskScheduler();
		}

		[OneTimeTearDown]
		public void TestFixtureTearDown()
		{
			_taskScheduler.Dispose();
		}

		[SetUp]
		public void Setup()
		{
			_logFile = new Mock<ILogSource>();
			_listeners = new LogSourceListenerCollection(_logFile.Object);
			_logFile.Setup(x => x.AddListener(It.IsAny<ILogSourceListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
					.Callback((ILogSourceListener listener, TimeSpan maximumWaitTime, int maximumLineCount) => _listeners.AddListener(listener, maximumWaitTime, maximumLineCount));
			_logFile.Setup(x => x.RemoveListener(It.IsAny<ILogSourceListener>()))
					.Callback((ILogSourceListener listener) => _listeners.RemoveListener(listener));

			_listener = new Mock<ILogSourceListener>();
			_modifications = new List<LogSourceModification>();
			_listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogSourceModification>()))
					 .Callback((ILogSource logFile, LogSourceModification modification) => _modifications.Add(modification));
		}

		[Test]
		[Description("Verifies that OnlogFileModified is eventually called when a non-zero maximum wait time is used (and the max limit is not reached)")]
		public void TestListen1()
		{
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.AddListener(_listener.Object, TimeSpan.FromSeconds(1), 1000);
				proxy.OnLogFileModified(_logFile.Object, LogSourceModification.Appended(0, 1));

				_modifications.Property(x => x.Count).ShouldEventually().Be(2);
				_modifications.Should().Equal(new[]
				{
					LogSourceModification.Reset(),
					LogSourceModification.Appended(0, 1)
				});
			}
		}
	}
}