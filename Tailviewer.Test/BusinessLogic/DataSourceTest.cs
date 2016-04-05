using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class DataSourceTest
	{
		[Test]
		public void TestCtor()
		{
			using (var source = new SingleDataSource(new DataSource(@"E:\somelogfile.txt") {Id = Guid.NewGuid()}))
			{
				source.FullFileName.Should().Be(@"E:\somelogfile.txt");
				source.LevelFilter.Should().Be(LevelFlags.All);
				source.StringFilter.Should().BeNull();
				source.FollowTail.Should().BeFalse();
			}
		}
	}
}