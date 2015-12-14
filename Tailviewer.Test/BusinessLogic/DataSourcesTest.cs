using System.Collections.ObjectModel;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;
using Tailviewer.Ui.ViewModels;
using DataSource = Tailviewer.Settings.DataSource;
using DataSources = Tailviewer.Settings.DataSources;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class DataSourcesTest
	{
		[Test]
		public void TestCtor()
		{
			var settings = new DataSources
				{
					new DataSource(@"E:\Code\test.log")
				};
			using (var dataSources = new Tailviewer.BusinessLogic.DataSources(settings))
			{
				dataSources.Count.Should().Be(1);
				var dataSource = dataSources.First();
				dataSource.FullFileName.Should().Be(settings[0].File);

				settings.Count.Should().Be(1);
				settings[0].File.Should().Be(@"E:\Code\test.log");
			}
		}

		[Test]
		public void TestAdd()
		{
			var settings = new DataSources();
			using (var dataSources = new Tailviewer.BusinessLogic.DataSources(settings))
			{
				var source = dataSources.Add(@"E:\Code\test.log");
				source.Should().NotBeNull();
				source.FullFileName.Should().Be(@"E:\Code\test.log");
				source.FollowTail.Should().BeFalse();

				settings.Count.Should().Be(1);
				settings[0].File.Should().Be(@"E:\Code\test.log");
			}
		}

		[Test]
		public void TestRemove()
		{
			var settings = new DataSources();
			using (var dataSources = new Tailviewer.BusinessLogic.DataSources(settings))
			{
				var source1 = dataSources.Add(@"E:\Code\test1.log");
				var source2 = dataSources.Add(@"E:\Code\test2.log");

				dataSources.Remove(source1);
				settings.Count.Should().Be(1);
				settings[0].File.Should().Be(@"E:\Code\test2.log");

				dataSources.Remove(source2);
				settings.Should().BeEmpty();
			}
		}
	}
}