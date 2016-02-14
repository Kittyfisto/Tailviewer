using System;
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
			using (var source = new SingleDataSource(new DataSource(@"E:\somelogfile.txt"){Id = Guid.NewGuid()}))
			{
				source.FullFileName.Should().Be(@"E:\somelogfile.txt");
				source.LevelFilter.Should().Be(LevelFlags.All);
				source.StringFilter.Should().BeNull();
				source.IsOpen.Should().BeFalse();
				source.FollowTail.Should().BeFalse();
			}
		}
	}
}