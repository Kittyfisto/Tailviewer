using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class EmptyLogFileTest
	{

		[Test]
		public void TestConstruction()
		{
			var logFile = new EmptyLogFile();
			logFile.EndOfSourceReached.Should().BeTrue();
			logFile.GetProperty(TextLogFileProperties.MaxCharactersInLine).Should().Be(0);
			logFile.GetProperty((IReadOnlyPropertyDescriptor)TextLogFileProperties.MaxCharactersInLine).Should().Be(0);
			logFile.Progress.Should().Be(1);
			logFile.Count.Should().Be(0);
			logFile.GetProperty(TextLogFileProperties.LineCount).Should().Be(0);
			logFile.GetProperty((IReadOnlyPropertyDescriptor)TextLogFileProperties.MaxCharactersInLine).Should().Be(0);
			logFile.Columns.Should().BeEquivalentTo(LogFileColumns.Minimum);
		}

		[Test]
		public void TestAddListener()
		{
			var logFile = new EmptyLogFile();
			var listener = new Mock<ILogFileListener>();
			logFile.AddListener(listener.Object, TimeSpan.Zero, 0);
			listener.Verify(x => x.OnLogFileModified(logFile, LogFileSection.Reset), Times.Once);
		}
	}
}
