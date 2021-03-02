using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Sources
{
	[TestFixture]
	public sealed class EmptyLogFileTest
	{

		[Test]
		public void TestConstruction()
		{
			var logFile = new EmptyLogSource();
			logFile.EndOfSourceReached.Should().BeTrue();
			logFile.GetProperty(TextProperties.MaxCharactersInLine).Should().Be(0);
			logFile.GetProperty((IReadOnlyPropertyDescriptor)TextProperties.MaxCharactersInLine).Should().Be(0);
			logFile.Progress.Should().Be(1);
			logFile.Count.Should().Be(0);
			logFile.GetProperty(TextProperties.LineCount).Should().Be(0);
			logFile.GetProperty((IReadOnlyPropertyDescriptor)TextProperties.MaxCharactersInLine).Should().Be(0);
			logFile.Columns.Should().BeEquivalentTo(GeneralColumns.Minimum);
		}

		[Test]
		public void TestAddListener()
		{
			var logFile = new EmptyLogSource();
			var listener = new Mock<ILogSourceListener>();
			logFile.AddListener(listener.Object, TimeSpan.Zero, 0);
			listener.Verify(x => x.OnLogFileModified(logFile, LogSourceModification.Reset()), Times.Once);
		}
	}
}
