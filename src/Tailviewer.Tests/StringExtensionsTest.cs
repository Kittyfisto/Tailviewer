using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;

namespace Tailviewer.Tests
{
	[TestFixture]
	public sealed class StringExtensionsTest
	{
		[Test]
		public void TestTrimNewlineEnd1()
		{
			string line = null;
			new Action(() => line.TrimNewlineEnd()).Should().NotThrow();
			line.TrimNewlineEnd().Should().BeNull();
		}

		[Test]
		public void TestTrimNewlineEnd2()
		{
			string line = string.Empty;
			new Action(() => line.TrimNewlineEnd()).Should().NotThrow();
			line.TrimNewlineEnd().Should().Be(string.Empty);
		}

		[Test]
		public void TestTrimNewlineEnd3()
		{
			string line = "Hello World\r";
			line.TrimNewlineEnd().Should().Be("Hello World\r");
		}

		[Test]
		public void TestTrimNewlineEnd4()
		{
			string line = "Hello World\r\n";
			line.TrimNewlineEnd().Should().Be("Hello World");
		}

		[Test]
		public void TestTrimNewlineEnd5()
		{
			string line = "Hello World\r\n ";
			line.TrimNewlineEnd().Should().Be("Hello World\r\n ");
		}

		[Test]
		public void TestTrimNewlineEnd6()
		{
			string line = "Hello World\r\n\t";
			line.TrimNewlineEnd().Should().Be("Hello World\r\n\t");
		}
	}
}