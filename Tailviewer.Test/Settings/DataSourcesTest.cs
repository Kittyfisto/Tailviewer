using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Settings;
using DataSources = Tailviewer.Settings.DataSources;

namespace Tailviewer.Test.Settings
{
	[TestFixture]
	public sealed class DataSourcesTest
	{
		[Test]
		public void TestClone()
		{
			var id = DataSourceId.CreateNew();
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

		[Test]
		public void TestMoveBefore1()
		{
			var dataSources = new DataSources();
			var a = new DataSource(@"A");
			var b = new DataSource(@"B");
			dataSources.Add(a);
			dataSources.Add(b);
			dataSources.MoveBefore(b, a);
			dataSources.Should().Equal(b, a);
		}

		[Test]
		[Description("Verifies that MoveBefore does not change the order of the list if the desired constraint already is true")]
		public void TestMoveBefore2()
		{
			var dataSources = new DataSources();
			var a = new DataSource(@"A");
			var b = new DataSource(@"B");
			dataSources.Add(a);
			dataSources.Add(b);
			dataSources.MoveBefore(a, b);
			dataSources.Should().Equal(a, b);
		}

		[Test]
		[Description("Verifies that MoveBefore does not change the order of the list if the desired constraint already is true")]
		public void TestMoveBefore3()
		{
			var dataSources = new DataSources();
			var a = new DataSource(@"A");
			var b = new DataSource(@"B");
			var c = new DataSource(@"C");
			dataSources.Add(a);
			dataSources.Add(b);
			dataSources.Add(c);
			dataSources.MoveBefore(a, c);
			dataSources.Should().Equal(a, b, c);
		}

		[Test]
		[Description("Verifies that MoveBefore doesn't do anything when either source or anchor don't exist")]
		public void TestMoveBefore4()
		{
			var dataSources = new DataSources();
			var a = new DataSource(@"A");
			var b = new DataSource(@"B");
			dataSources.Add(a);
			new Action(() => dataSources.MoveBefore(a, b)).ShouldNotThrow();
			dataSources.Should().Equal(a);
			new Action(() => dataSources.MoveBefore(b, a)).ShouldNotThrow();
			dataSources.Should().Equal(a);
		}

		[Test]
		[Description("Verifies that MoveBefore doesn't do anything when either source or anchor don't exist")]
		public void TestMoveBefore5()
		{
			var dataSources = new DataSources();
			var a = new DataSource(@"A");
			var b = new DataSource(@"B");
			var c = new DataSource(@"C");
			dataSources.Add(a);
			new Action(() => dataSources.MoveBefore(b, c)).ShouldNotThrow();
			dataSources.Should().Equal(a);
		}
	}
}