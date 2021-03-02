using System;
using FluentAssertions;
using NUnit.Framework;

namespace Tailviewer.Api.Tests
{
	[TestFixture]
	public abstract class AbstractIdTest<T>
		where T : struct, IEquatable<T>
	{
		protected abstract T Empty();
		protected abstract T CreateNew();

		[Test]
		public void TestEmpty()
		{
			var id1 = Empty();
			var id2 = Empty();

			id1.Equals(id2).Should().BeTrue();
			((object) id1).Equals(id2).Should().BeTrue();
		}

		[Test]
		public void TestCreateNew1()
		{
			var id1 = CreateNew();
			var id2 = CreateNew();

			id1.Equals(id2).Should().BeFalse();
			((object) id1).Equals(id2).Should().BeFalse();
			id1.ToString().Should().NotBe(id2.ToString());
		}

		[Test]
		public void TestEquals()
		{
			var id = CreateNew();
			id.Equals(null).Should().BeFalse();
			id.Equals(1).Should().BeFalse();
			id.Equals(id).Should().BeTrue();
			id.GetHashCode().Should().Be(id.GetHashCode());
		}

		[Test]
		public void TestGetHashCode()
		{
			var id = CreateNew();
			id.GetHashCode().Should().Be(id.GetHashCode());
		}
	}
}