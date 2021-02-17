using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Core.Sources;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class LogFileListenerCollectionTest
	{
		[Test]
		[Description(
			"Verifies that AddListener may be called multiple times, but if it is, then events aren't fired multiple times for each invocation"
			)]
		public void TestAddListener1()
		{
			ILogSource logSource = new Mock<ILogSource>().Object;
			var collection = new LogSourceListenerCollection(logSource);
			var listener = new Mock<ILogSourceListener>();
			var sections = new List<LogFileSection>();
			listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogFileSection>()))
			        .Callback((ILogSource file, LogFileSection y) => sections.Add(y));

			collection.AddListener(listener.Object, TimeSpan.FromSeconds(1), 10);
			new Action(() => collection.AddListener(listener.Object, TimeSpan.FromSeconds(1), 10)).Should().NotThrow();

			collection.OnRead(10);
			sections.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 10)
				}, "Because even though we added the listener twice, it should never be invoked twice");
		}

		[Test]
		public void TestInvalidate()
		{
			var collection = new LogSourceListenerCollection(new Mock<ILogSource>().Object);
			collection.OnRead(1);
			collection.CurrentLineIndex.Should().Be(1);
			collection.Invalidate(0, 1);
			collection.CurrentLineIndex.Should().Be(0);
		}

		[Test]
		[Description("Verifies that Flush() forces calling the OnLogFileModified method, even when neither the maximum amount of lines has been reached, nor the maximum amount of time has ellapsed")]
		public void TestFlush1()
		{
			var collection = new LogSourceListenerCollection(new Mock<ILogSource>().Object);

			var listener = new Mock<ILogSourceListener>();
			var sections = new List<LogFileSection>();
			listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogFileSection>()))
					.Callback((ILogSource file, LogFileSection y) => sections.Add(y));

			collection.AddListener(listener.Object, TimeSpan.FromHours(1), 1000);
			collection.OnRead(1);

			sections.Should().Equal(new object[]
				{
					LogFileSection.Reset
				});

			collection.Flush();
			sections.Should().Equal(new object[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 1)
				}, "Because Flush() should force calling the OnLogFileModified method");
		}

		[Test]
		public void TestFlush2()
		{
			var collection = new LogSourceListenerCollection(new Mock<ILogSource>().Object);

			var listener = new Mock<ILogSourceListener>();
			var sections = new List<LogFileSection>();
			listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogFileSection>()))
					.Callback((ILogSource file, LogFileSection y) => sections.Add(y));

			collection.AddListener(listener.Object, TimeSpan.FromHours(1), 1000);
			collection.OnRead(1);

			collection.Flush();
			collection.Flush();
			sections.Should().Equal(new object[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 1)
				}, "Because Flush() shouldn't forward the same result to the same listener more than once");
		}

		[Test]
		public void TestFlush3()
		{
			var collection = new LogSourceListenerCollection(new Mock<ILogSource>().Object);

			var listener = new Mock<ILogSourceListener>();
			var sections = new List<LogFileSection>();
			listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogFileSection>()))
					.Callback((ILogSource file, LogFileSection y) => sections.Add(y));

			collection.AddListener(listener.Object, TimeSpan.FromHours(1), 1000);
			collection.OnRead(1);
			collection.Flush();
			collection.OnRead(2);
			collection.Flush();
			sections.Should().Equal(new object[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 1),
					new LogFileSection(1, 1)
				});
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/282")]
		public void TestClear()
		{
			var collection = new LogSourceListenerCollection(new Mock<ILogSource>().Object);

			var listener = new Mock<ILogSourceListener>();
			var sections = new List<LogFileSection>();
			listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogFileSection>()))
			        .Callback((ILogSource file, LogFileSection y) => sections.Add(y));

			collection.AddListener(listener.Object, TimeSpan.FromHours(1), 1000);
			collection.OnRead(1);
			collection.Flush();
			sections.Should().Equal(new object[]
			{
				LogFileSection.Reset,
				new LogFileSection(0, 1),
			});

			collection.Clear();
			sections.Clear();

			collection.OnRead(2);
			collection.Flush();
			sections.Should()
			        .BeEmpty("because the collection should have removed all listeners and thus we may not have been notified anymore");
		}
	}
}