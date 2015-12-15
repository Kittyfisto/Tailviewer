using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using DataSource = Tailviewer.Settings.DataSource;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class DataSourceTest
	{
		[Test]
		public void TestCtor()
		{
			using (var source = new Tailviewer.BusinessLogic.DataSource(new DataSource(@"E:\somelogfile.txt")))
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