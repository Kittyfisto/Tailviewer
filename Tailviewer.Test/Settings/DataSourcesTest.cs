using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;

namespace Tailviewer.Test.Settings
{
	[TestFixture]
	public sealed class DataSourcesTest
	{
		[Test]
		public void TestClone()
		{
			var id = Guid.NewGuid();
			var dataSources = new DataSources
			{
				new DataSource(@"A:\stuff")
			};
			dataSources.SelectedItem = id;
			var cloned = dataSources.Clone();
			cloned.Should().NotBeNull();
			cloned.Should().NotBeSameAs(dataSources);
			cloned.SelectedItem.Should().Be(id);
			cloned.Count.Should().Be(1);
			cloned[0].Should().NotBeNull();
			cloned[0].Should().NotBeSameAs(dataSources[0]);
			cloned[0].File.Should().Be(@"A:\stuff");
		}
	}
}