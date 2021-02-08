using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Core.Parsers;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class TextLogFileParserTest
	{
		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/273")]
		public void TestRemoveGarbageCharacters()
		{
			var rawContent = "\0\0\02021-02-08 07:58:48,060 [0x000025d0] DEBUG foo Well I'll be buggered, where did those NULs come from?";
			var parser = new TextLogFileParser(new TimestampParser());
			var parsedLogEntry = parser.Parse(new LogEntry2 {RawContent = rawContent});
			parsedLogEntry.RawContent.Should()
			              .Be("2021-02-08 07:58:48,060 [0x000025d0] DEBUG foo Well I'll be buggered, where did those NULs come from?");
			parsedLogEntry.Timestamp.Should().Be(new DateTime(2021, 2, 8, 7, 58, 48, 060));
		}
	}
}
