using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Parsers;

namespace Tailviewer.Test.BusinessLogic.LogFiles.Parsers
{
	[TestFixture]
	public sealed class TimeOfDaySecondsSinceStartParserTest
	{
		[Test]
		public void TestTryParse1()
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
		public void TestTryParse2()
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

		[Test]
		public void TestTryParse3()
		{
			var parser = new TimeOfDaySecondsSinceStartParser();
			DateTime timestamp;

			const string content = "2017-03-24 11-45-22.182783; 0; 0;  0; 109;  0; 125;   1;PB_CONTINUE; ; ; 109; 2;   2.30; 0; S.N. 100564: 0.3 sec for:";
			new Action(() => parser
					.TryParse(
						content,
						out timestamp)).Should().NotThrow();

			parser.TryParse(content, out timestamp).Should().BeFalse();
		}

		[Test]
		public void TestTryParse4()
		{
			var parser = new TimeOfDaySecondsSinceStartParser();

			DateTime timestamp;

			const string content = " \"c:\\windows\\syswow64\\\\windowspowershell\\v1.0\\powershell.exe\" -NonInteractive -NoLogo -NoProfile -ExecutionPolicy Bypass -InputFormat None \"$ErrorActionPreference=\"\"\"Stop\"\"\"; $VerbosePreference=\"\"\"Continue\"\"\"; $CeipSetting=\"\"\"on\"\"\"; $ScriptPath=\"\"\"C:\\";
			parser.TryParse(content, out timestamp).Should().BeFalse();
			timestamp.Should().Be(DateTime.MinValue);
		}
	}
}