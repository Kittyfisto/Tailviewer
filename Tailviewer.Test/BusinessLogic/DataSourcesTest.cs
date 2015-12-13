using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class DataSourcesTest
	{
		[Test]
		public void TestCtor()
		{
			var settings = new DataSourcesSettings
				{
					new DataSourceSettings(@"E:\Code\test.log")
				};
			var dataSources = new DataSources(settings);
			dataSources.Count.Should().Be(1);
			var dataSource = dataSources.First();
			dataSource.FullFileName.Should().Be(settings[0].File);

			settings.Count.Should().Be(1);
			settings[0].File.Should().Be(@"E:\Code\test.log");
		}

		[Test]
		public void TestAdd()
		{
			var settings = new DataSourcesSettings();
			var dataSources = new DataSources(settings);
			var source = dataSources.Add(@"E:\Code\test.log");
			source.Should().NotBeNull();
			source.FullFileName.Should().Be(@"E:\Code\test.log");
			source.FollowTail.Should().BeFalse();

			settings.Count.Should().Be(1);
			settings[0].File.Should().Be(@"E:\Code\test.log");
		}
	}
}