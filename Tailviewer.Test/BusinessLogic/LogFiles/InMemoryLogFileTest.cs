using System;
using FluentAssertions;
using Metrolib;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class InMemoryLogFileTest
	{
		[Test]
		public void TestConstruction1()
		{
			var logFile = new InMemoryLogFile();
			logFile.FileSize.Should().Be(Size.Zero);
			logFile.MaxCharactersPerLine.Should().Be(0);
			logFile.LastModified.Should().Be(DateTime.MinValue);
			logFile.StartTimestamp.Should().BeNull();
			logFile.Exists.Should().BeTrue();
			logFile.EndOfSourceReached.Should().BeTrue();
			logFile.Count.Should().Be(0);
		}

		[Test]
		public void TestAddEntry1()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Hello, World!", LevelFlags.Info);
			logFile.Count.Should().Be(1);
			logFile.MaxCharactersPerLine.Should().Be(13);
		}

		[Test]
		public void TestAddEntry2()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Hello,", LevelFlags.Info, new DateTime(2017, 4, 29, 14, 56, 0));
			logFile.AddEntry(" World!", LevelFlags.Warning, new DateTime(2017, 4, 29, 14, 56, 2));
			logFile.Count.Should().Be(2);
			logFile.GetLine(0).Should().Be(new LogLine(0, 0, "Hello,", LevelFlags.Info, new DateTime(2017, 4, 29, 14, 56, 0)));
			logFile.GetLine(1).Should().Be(new LogLine(1, 1, " World!", LevelFlags.Warning, new DateTime(2017, 4, 29, 14, 56, 2)));
		}

		[Test]
		[Description("Verifies that the MaxCharactersPerLine doesn't decrease")]
		public void TestAddEntry3()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Hello, World!", LevelFlags.Info);
			logFile.AddEntry("Hi", LevelFlags.Info);
			logFile.MaxCharactersPerLine.Should().Be(13);
		}

		[Test]
		[Description("Verifies that the MaxCharactersPerLine only increases")]
		public void TestAddEntry4()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Hi", LevelFlags.Info);
			logFile.MaxCharactersPerLine.Should().Be(2);
			logFile.AddEntry("Hello, World!", LevelFlags.Info);
			logFile.MaxCharactersPerLine.Should().Be(13);
		}

		[Test]
		[Description("Verifies that clear works on an empty file and doesn't do anything")]
		public void TestClear1()
		{
			var logFile = new InMemoryLogFile();
			logFile.Clear();
			logFile.FileSize.Should().Be(Size.Zero);
			logFile.MaxCharactersPerLine.Should().Be(0);
			logFile.LastModified.Should().Be(DateTime.MinValue);
			logFile.StartTimestamp.Should().BeNull();
			logFile.Exists.Should().BeTrue();
			logFile.EndOfSourceReached.Should().BeTrue();
			logFile.Count.Should().Be(0);
		}

		[Test]
		[Description("Verifies that Clear actually removes lines")]
		public void TestClear2()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Hello,", LevelFlags.Info);
			logFile.AddEntry(" World!", LevelFlags.Warning);
			logFile.Clear();
			logFile.Count.Should().Be(0);
		}

		[Test]
		[Description("Verifies that the MaxCharactersPerLine is reset when Clear()ed")]
		public void TestClear3()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Hi", LevelFlags.Info);
			logFile.MaxCharactersPerLine.Should().Be(2);
			logFile.Clear();
			logFile.MaxCharactersPerLine.Should().Be(0);
		}
	}
}