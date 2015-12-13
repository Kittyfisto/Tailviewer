using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class DataSourceTest
	{
		[Test]
		public void TestCtor()
		{
			using (var source = new DataSource(new DataSourceSettings(@"E:\somelogfile.txt")))
			{
				source.FullFileName.Should().Be(@"E:\somelogfile.txt");
				source.LevelFilter.Should().Be(LevelFlags.All);
				source.StringFilter.Should().BeNull();
				source.IsOpen.Should().BeFalse();
				source.FollowTail.Should().BeFalse();
				source.OtherFilter.Should().BeFalse();
			}
		}
	}
}