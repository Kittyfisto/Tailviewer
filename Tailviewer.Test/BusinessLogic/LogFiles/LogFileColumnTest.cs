using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogFileColumnTest
	{
		[Test]
		public void TestToString()
		{
			var column = new WellKnownLogFileColumn<string>("foobar", "Foobar");
			column.ToString().Should().Be("foobar");
		}
	}
}
