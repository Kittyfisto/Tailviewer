using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core.Entries;
using Tailviewer.Core.Parsers;

namespace Tailviewer.Test.BusinessLogic.LogFiles.Text
{
	[TestFixture]
	public sealed class GenericTextLogEntryParserTest
	{
		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/273")]
		public void TestRemoveGarbageCharacters()
		{
			var rawContent = "\0\0\02021-02-08 07:58:48,060 [0x000025d0] DEBUG foo Well I'll be buggered, where did those NULs come from?";
			var parser = new GenericTextLogEntryParser(new TimestampParser());
			var parsedLogEntry = parser.Parse(new LogEntry {RawContent = rawContent});
			parsedLogEntry.RawContent.Should()
			              .Be("2021-02-08 07:58:48,060 [0x000025d0] DEBUG foo Well I'll be buggered, where did those NULs come from?");
			parsedLogEntry.Timestamp.Should().Be(new DateTime(2021, 2, 8, 7, 58, 48, 060));
		}

		[Test]
		public void TestRemoveUnprintableCharacters()
		{
			var unprintableCharacters = new[]
			{
				'\0', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\a', '\b', '\n', '\v', '\f', '\r',
				'\u000e', '\u000f', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001e', '\u001f',
				'\u007f'
			};
			foreach (var character in unprintableCharacters)
			{
				var rawContent = $"Foo{character}bar";
				var parser = new GenericTextLogEntryParser(new TimestampParser());
				var parsedLogEntry = parser.Parse(new LogEntry {RawContent = rawContent});
				parsedLogEntry.RawContent.Should()
				              .Be("Foobar");
			}
		}

		[Test]
		public void TestDontRemoveTab()
		{
			var rawContent = "Foo\tbar";
			var parser = new GenericTextLogEntryParser(new TimestampParser());
			var parsedLogEntry = parser.Parse(new LogEntry {RawContent = rawContent});
			parsedLogEntry.RawContent.Should()
			              .Be("Foo\tbar");
		}

		[Test]
		public void TestDontRemoveSpace()
		{
			var rawContent = "Foo bar";
			var parser = new GenericTextLogEntryParser(new TimestampParser());
			var parsedLogEntry = parser.Parse(new LogEntry {RawContent = rawContent});
			parsedLogEntry.RawContent.Should()
			              .Be("Foo bar");
		}
	}
}
