﻿using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Sources;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogFileProxyTest
	{
		private DefaultTaskScheduler _scheduler;
		private Mock<ILogSource> _logFile;
		private LogSourceListenerCollection _listeners;
		private Mock<ILogSourceListener> _listener;
		private List<LogFileSection> _modifications;

		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			_scheduler = new DefaultTaskScheduler();
		}

		[OneTimeTearDown]
		public void TestFixtureTearDown()
		{
			_scheduler.Dispose();
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
			_modifications = new List<LogFileSection>();
			_listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogFileSection>()))
					 .Callback((ILogSource logFile, LogFileSection section) => _modifications.Add(section));
		}

		[Test]
		[Description("Verifies that OnlogFileModified is eventually called when a non-zero maximum wait time is used (and the max limit is not reached)")]
		public void TestListen1()
		{
			using (var proxy = new LogSourceProxy(_scheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.AddListener(_listener.Object, TimeSpan.FromSeconds(1), 1000);
				proxy.OnLogFileModified(_logFile.Object, new LogFileSection(0, 1));

				_modifications.Property(x => x.Count).ShouldEventually().Be(2);
				_modifications.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 1)
				});
			}
		}
	}
}