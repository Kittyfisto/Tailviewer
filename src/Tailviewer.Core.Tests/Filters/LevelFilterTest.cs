using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Filters
{
	[TestFixture]
	public sealed class LevelFilterTest
	{
		[Test]
		public void TestToString()
		{
			new LevelFilter(LevelFlags.Debug).ToString().Should().Be("level == Debug");
			new LevelFilter(LevelFlags.Debug | LevelFlags.Info).ToString().Should().Be("(level == Debug || Info)");
		}
	}
}