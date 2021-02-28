using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Filters.ExpressionEngine;

namespace Tailviewer.Test.BusinessLogic.Filters.ExpressionEngine
{
	[TestFixture]
	public sealed class LogLevelLiteralTest
	{
		[Test]
		public void TestTryParseFatal([Values("fatal", "FATAL", "Fatal")] string value)
		{
			LogLevelLiteral.TryParse(value, out var logLevel).Should().BeTrue();
			logLevel.Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestTryParseError([Values("error", "ERROR", "Error")] string value)
		{
			LogLevelLiteral.TryParse(value, out var logLevel).Should().BeTrue();
			logLevel.Should().Be(LevelFlags.Error);
		}

		[Test]
		public void TestTryParseWarning([Values("warning", "WARNING", "Warning")] string value)
		{
			LogLevelLiteral.TryParse(value, out var logLevel).Should().BeTrue();
			logLevel.Should().Be(LevelFlags.Warning);
		}

		[Test]
		public void TestTryParseInfo([Values("info", "INFO", "Info")] string value)
		{
			LogLevelLiteral.TryParse(value, out var logLevel).Should().BeTrue();
			logLevel.Should().Be(LevelFlags.Info);
		}

		[Test]
		public void TestTryParseDebug([Values("debug", "DEBUG", "Debug")] string value)
		{
			LogLevelLiteral.TryParse(value, out var logLevel).Should().BeTrue();
			logLevel.Should().Be(LevelFlags.Debug);
		}

		[Test]
		public void TestTryParseTrace([Values("trace", "TRACE", "Trace")] string value)
		{
			LogLevelLiteral.TryParse(value, out var logLevel).Should().BeTrue();
			logLevel.Should().Be(LevelFlags.Trace);
		}

		[Test]
		public void TestTryParseNone([Values("other", "OTHER", "Other")] string value)
		{
			LogLevelLiteral.TryParse(value, out var logLevel).Should().BeTrue();
			logLevel.Should().Be(LevelFlags.Other);
		}

		[Test]
		public void TestTryParseUnknown([Values("swdwd", "", null)] string value)
		{
			LogLevelLiteral.TryParse(value, out var logLevel).Should().BeFalse();
			logLevel.Should().Be(LevelFlags.None);
		}
	}
}
