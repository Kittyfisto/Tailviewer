﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core;
using Tailviewer.Core.Tests.Sources;

namespace Tailviewer.Tests.BusinessLogic.Sources
{
	[TestFixture]
	public sealed class NoThrowLogSourceTest
		: AbstractLogSourceTest
	{
		private Mock<ILogSource> _logFile;
		private string _pluginName;
		private NoThrowLogSource _proxy;

		[SetUp]
		public void Setup()
		{
			_logFile = new Mock<ILogSource>();
			_pluginName = "Buggy plugin";
			_proxy = new NoThrowLogSource(_logFile.Object, _pluginName);
		}

		[Test]
		public void TestDispose()
		{
			_logFile.Setup(x => x.Dispose()).Throws<SystemException>();
			new Action(() => _proxy.Dispose()).Should().NotThrow();
			_logFile.Verify(x => x.Dispose(), Times.Once);
		}

		[Test]
		public void TestStartTimestamp()
		{
			_logFile.Setup(x => x.GetProperty(Properties.StartTimestamp)).Throws<SystemException>();
			_proxy.GetProperty(Properties.StartTimestamp).Should().BeNull();
			_logFile.Verify(x => x.GetProperty(Properties.StartTimestamp), Times.Once);
		}

		[Test]
		public void TestLastModified()
		{
			_logFile.Setup(x => x.GetProperty(Properties.LastModified)).Throws<SystemException>();
			new Action(() =>
			{
				var unused = _proxy.GetProperty(Properties.LastModified);
			}).Should().NotThrow();
			_logFile.Verify(x => x.GetProperty(Properties.LastModified), Times.Once);
		}

		[Test]
		public void TestExists()
		{
			_logFile.Setup(x => x.GetProperty(Properties.EmptyReason)).Throws<SystemException>();
			new Action(() =>
			{
				var unused = _proxy.GetProperty(Properties.EmptyReason);
			}).Should().NotThrow();
			_logFile.Verify(x => x.GetProperty(Properties.EmptyReason), Times.Once);
		}

		[Test]
		public void TestCount()
		{
			_logFile.Setup(x => x.GetProperty(Properties.LogEntryCount)).Throws<SystemException>();
			new Action(() =>
			{
				var unused = _proxy.GetProperty(Properties.LogEntryCount);
			}).Should().NotThrow();
			_logFile.Verify(x => x.GetProperty(Properties.LogEntryCount), Times.Once);
		}

		[Test]
		public void TestAddListener()
		{
			_logFile.Setup(x => x.AddListener(It.IsAny<ILogSourceListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>())).Throws<SystemException>();
			var listener = new Mock<ILogSourceListener>().Object;
			var maximumWaitTime = TimeSpan.FromSeconds(42);
			var maximumLineCount = 9001;
			new Action(() => _proxy.AddListener(listener, maximumWaitTime, maximumLineCount)).Should().NotThrow();
			_logFile.Verify(x => x.AddListener(It.Is<ILogSourceListener>(y => y == listener),
				It.Is<TimeSpan>(y => y == maximumWaitTime),
				It.Is<int>(y => y == maximumLineCount)), Times.Once);
		}

		[Test]
		public void TestRemoveListener()
		{
			_logFile.Setup(x => x.RemoveListener(It.IsAny<ILogSourceListener>())).Throws<SystemException>();
			var listener = new Mock<ILogSourceListener>().Object;
			new Action(() => _proxy.RemoveListener(listener)).Should().NotThrow();
			_logFile.Verify(x => x.RemoveListener(It.Is<ILogSourceListener>(y => y == listener)), Times.Once);
		}

		[Test]
		public void TestGetLogLineIndexOfOriginalLineIndex()
		{
			_logFile.Setup(x => x.GetLogLineIndexOfOriginalLineIndex(It.IsAny<LogLineIndex>())).Throws<SystemException>();
			var index = new LogLineIndex(42);
			new Action(() => _proxy.GetLogLineIndexOfOriginalLineIndex(index)).Should().NotThrow();
			_logFile.Verify(x => x.GetLogLineIndexOfOriginalLineIndex(It.Is<LogLineIndex>(y => y == index)), Times.Once);
		}

		[Test]
		public void TestGetColumn1()
		{
			_logFile.Setup(x => x.GetColumn(It.IsAny<LogSourceSection>(), It.IsAny<IColumnDescriptor<string>>(), It.IsAny<string[]>(), It.IsAny<int>(), It.IsAny<LogSourceQueryOptions>())).Throws<SystemException>();

			var section = new LogSourceSection(42, 100);
			var buffer = new string[9101];
			var destinationIndex = 9001;
			var queryOptions = new LogSourceQueryOptions(LogSourceQueryMode.FromCache);
			new Action(() => _proxy.GetColumn(section, Columns.RawContent, buffer, destinationIndex, queryOptions)).Should().NotThrow();

			_logFile.Verify(x => x.GetColumn(It.Is<LogSourceSection>(y => y == section),
			                                 Columns.RawContent,
			                                 buffer,
			                                 destinationIndex,
			                                 queryOptions),
			                Times.Once);
		}

		[Test]
		public void TestGetColumn2()
		{
			_logFile.Setup(x => x.GetColumn(It.IsAny<IReadOnlyList<LogLineIndex>>(), It.IsAny<IColumnDescriptor<string>>(), It.IsAny<string[]>(), It.IsAny<int>(), It.IsAny<LogSourceQueryOptions>())).Throws<SystemException>();

			var indices = new LogLineIndex[] {1, 2, 3};
			var buffer = new string[201];
			var queryOptions = new LogSourceQueryOptions(LogSourceQueryMode.FromCache);
			new Action(() => _proxy.GetColumn(indices, Columns.RawContent, buffer, 101, queryOptions)).Should().NotThrow();

			_logFile.Verify(x => x.GetColumn(It.Is<IReadOnlyList<LogLineIndex>>(y => y == indices),
			                                 It.Is<IColumnDescriptor<string>>(y => Equals(y, Columns.RawContent)),
			                                 It.Is<string[]>(y => ReferenceEquals(y, buffer)),
			                                 It.Is<int>(y => y == 101),
			                                 queryOptions),
			                Times.Once);
		}

		[Test]
		public void TestGetEntries1()
		{
			_logFile.Setup(x => x.GetEntries(It.IsAny<LogSourceSection>(), It.IsAny<ILogBuffer>(), It.IsAny<int>(), It.IsAny<LogSourceQueryOptions>())).Throws<SystemException>();

			var section = new LogSourceSection(42, 100);
			var buffer = new Mock<ILogBuffer>().Object;
			var destinationIndex = 9001;
			var queryOptions = new LogSourceQueryOptions(LogSourceQueryMode.FromCache);
			new Action(() => _proxy.GetEntries(section, buffer, destinationIndex, queryOptions)).Should().NotThrow();

			_logFile.Verify(x => x.GetEntries(It.Is<LogSourceSection>(y => y == section),
			                                 It.Is<ILogBuffer>(y => ReferenceEquals(y, buffer)),
			                                 destinationIndex,
			                                 queryOptions),
			                Times.Once);
		}

		[Test]
		public void TestGetEntries2()
		{
			_logFile.Setup(x => x.GetEntries(It.IsAny<IReadOnlyList<LogLineIndex>>(), It.IsAny<ILogBuffer>(), It.IsAny<int>(), It.IsAny<LogSourceQueryOptions>())).Throws<SystemException>();

			var indices = new LogLineIndex[] { 1, 2, 3 };
			var buffer = new Mock<ILogBuffer>().Object;
			var destinationIndex = 101;
			var queryOptions = new LogSourceQueryOptions(LogSourceQueryMode.FromCache);
			new Action(() => _proxy.GetEntries(indices, buffer, destinationIndex, queryOptions)).Should().NotThrow();

			_logFile.Verify(x => x.GetEntries(It.Is<IReadOnlyList<LogLineIndex>>(y => y == indices),
			                                 It.Is<ILogBuffer>(y => ReferenceEquals(y, buffer)),
			                                 destinationIndex,
			                                 queryOptions),
			                Times.Once);
		}

		[Test]
		public void TestGetColumns1()
		{
			_logFile.Setup(x => x.Columns).Returns(new[] {Columns.DeltaTime, Columns.ElapsedTime});
			_proxy.Columns.Should().Equal(Columns.DeltaTime, Columns.ElapsedTime);
			_logFile.Verify(x => x.Columns, Times.Once);
		}

		[Test]
		public void TestGetColumns2()
		{
			_logFile.Setup(x => x.Columns).Throws<NullReferenceException>();
			_proxy.Columns.Should().BeEmpty();
			_logFile.Verify(x => x.Columns, Times.Once);
		}

		protected override ILogSource CreateEmpty()
		{
			var source = new InMemoryLogSource();
			return new NoThrowLogSource(source, "");
		}

		protected override ILogSource CreateFromContent(IReadOnlyLogBuffer content)
		{
			var source = new InMemoryLogSource(content);
			return new NoThrowLogSource(source, "");
		}
	}
}