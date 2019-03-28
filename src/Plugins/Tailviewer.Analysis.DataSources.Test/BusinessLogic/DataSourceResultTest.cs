using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Analysis.DataSources.BusinessLogic;
using Tailviewer.Core;

namespace Tailviewer.Analysis.DataSources.Test.BusinessLogic
{
	[TestFixture]
	public sealed class DataSourceResultTest
	{
		[Test]
		public void TestClone()
		{
			var result = new DataSourceResult
			{
				Id = DataSourceId.CreateNew(),
				Name = "fpmawdmaw",
				SizeInBytes = 8112412198033L,
				Created = DateTime.Now,
				LastModified = new DateTime(2019, 3, 28, 19, 34, 01)
			};
			var actualResult = result.Clone();
			actualResult.Id.Should().Be(result.Id);
			actualResult.Name.Should().Be(result.Name);
			actualResult.SizeInBytes.Should().Be(result.SizeInBytes);
			actualResult.Created.Should().Be(result.Created);
			actualResult.LastModified.Should().Be(result.LastModified);
		}

		[Test]
		public void TestRoundtrip()
		{
			var result = new DataSourceResult
			{
				Id = DataSourceId.CreateNew(),
				Name = "fpmawdmaw",
				SizeInBytes = 8112412198033L,
				Created = DateTime.Now,
				LastModified = new DateTime(2019, 3, 28, 19, 34, 01)
			};
			var actualResult = result.Roundtrip();
			actualResult.Id.Should().Be(result.Id);
			actualResult.Name.Should().Be(result.Name);
			actualResult.SizeInBytes.Should().Be(result.SizeInBytes);
			actualResult.Created.Should().Be(result.Created);
			actualResult.LastModified.Should().Be(result.LastModified);
		}
	}
}
