using FluentAssertions;
using NUnit.Framework;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class LevelFlagsTest
	{
		[Test]
		public void TestAll()
		{
			LevelFlags.All.Should().Be(LevelFlags.Fatal | LevelFlags.Error | LevelFlags.Warning | LevelFlags.Info |
			                           LevelFlags.Debug | LevelFlags.Trace | LevelFlags.Other);
		}
	}
}