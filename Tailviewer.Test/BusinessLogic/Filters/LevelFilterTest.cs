using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Core.Filters;

namespace Tailviewer.Test.BusinessLogic.Filters
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