using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Analysis.DataSources.BusinessLogic;
using Tailviewer.Core;

namespace Tailviewer.Analysis.DataSources.Test.BusinessLogic
{
	[TestFixture]
	public sealed class DataSourcesResultTest
	{
		[Test]
		public void TestClone()
		{
			var result = new DataSourcesResult
			{
				DataSources =
				{
					new DataSourceResult
					{
						Name = "Stuff.txt"
					},
					new DataSourceResult
					{
						Name = "fooBar"
					}
				}
			};

			var actualResult = result.Clone();
			actualResult.Should().NotBeNull();
			actualResult.Should().NotBeSameAs(result);
			actualResult.DataSources.Should().NotBeSameAs(result.DataSources);
			actualResult.DataSources.Should().HaveCount(2);
			actualResult.DataSources[0].Should().NotBeSameAs(result.DataSources[0]);
			actualResult.DataSources[0].Name.Should().Be("Stuff.txt");
			actualResult.DataSources[1].Should().NotBeSameAs(result.DataSources[1]);
			actualResult.DataSources[1].Name.Should().Be("fooBar");
		}

		[Test]
		public void TestRoundtripEmpty()
		{
			var result = new DataSourcesResult();
			var actualResult = Roundtrip(result);
			actualResult.Should().NotBeNull();
			actualResult.DataSources.Should().NotBeNull();
			actualResult.DataSources.Should().BeEmpty();
		}

		[Test]
		public void TestRoundtripOneQuickFilter()
		{
			var result = new DataSourcesResult
			{
				DataSources =
				{
					new DataSourceResult
					{
						Name = "Stuff.txt"
					}
				}
			};
			var actualResult = Roundtrip(result);
			actualResult.Should().NotBeNull();
			actualResult.DataSources.Should().NotBeNull();
			actualResult.DataSources.Should().HaveCount(1);

			var dataSource = actualResult.DataSources.First();
			dataSource.Name.Should().Be("Stuff.txt");
		}

		private DataSourcesResult Roundtrip(DataSourcesResult config)
		{
			return config.Roundtrip(typeof(DataSourcesResult), typeof(DataSourceResult));
		}
	}
}
