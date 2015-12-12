using FluentAssertions;
using NUnit.Framework;
using SharpTail.BusinessLogic;

namespace SharpTail.Test.BusinessLogic
{
	[TestFixture]
	public sealed class DataSourceTest
	{
		[Test]
		public void TestCtor1()
		{
			var source = new DataSource();
			source.FullFileName.Should().BeNull();
			source.Levels.Should().Be(LevelFlags.All);
			source.StringFilter.Should().BeNull();
			source.IsOpen.Should().BeFalse();
			source.FollowTail.Should().BeFalse();
		}

		[Test]
		public void TestCtor2()
		{
			var source = new DataSource(@"E:\somelogfile.txt");
			source.FullFileName.Should().Be(@"E:\somelogfile.txt");
			source.Levels.Should().Be(LevelFlags.All);
			source.StringFilter.Should().BeNull();
			source.IsOpen.Should().BeFalse();
			source.FollowTail.Should().BeFalse();
		}
	}
}