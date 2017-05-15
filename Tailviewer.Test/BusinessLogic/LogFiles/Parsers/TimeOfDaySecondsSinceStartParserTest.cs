using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles.Parsers;

namespace Tailviewer.Test.BusinessLogic.LogFiles.Parsers
{
	[TestFixture]
	public sealed class TimeOfDaySecondsSinceStartParserTest
	{
		[Test]
		public void TestTryParseFirst1()
		{
			var parser = new TimeOfDaySecondsSinceStartParser();
			DateTime timestamp;
			parser.TryParse(null, out timestamp).Should().BeFalse();
			parser.TryParse("", out timestamp).Should().BeFalse();
			parser.TryParse("::;;", out timestamp).Should().BeFalse();
			parser.TryParse("0:0:0;;", out timestamp).Should().BeFalse();
			parser.TryParse("06", out timestamp).Should().BeFalse();
		}

		[Test]
		public void TestTryParseFirst2()
		{
			var parser = new TimeOfDaySecondsSinceStartParser();
			DateTime timestamp;
			parser.TryParse(
					"06:51:57 ;      0.135345; Foo size               0; Th  6252(0x186c); Start;MC   14; Id  169= 169[ 0]; Bar; ",
					out timestamp)
				.Should()
				.BeTrue();

			var today = DateTime.Today;
			timestamp.Year.Should().Be(today.Year);
			timestamp.Month.Should().Be(today.Month);
			timestamp.Day.Should().Be(today.Day);
			timestamp.Hour.Should().Be(6);
			timestamp.Minute.Should().Be(51);
			timestamp.Second.Should().Be(57);
			timestamp.Millisecond.Should().Be(135);
		}
	}
}