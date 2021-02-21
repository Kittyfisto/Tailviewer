using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class FixedSizeListTest
	{
		[Test]
		public void TestCreateEmpty()
		{
			var fixedList = new FixedSizeList<int>(10);
			fixedList.Count.Should().Be(0);
			fixedList.Buffer.Length.Should().Be(10);
			for (int i = 0; i < fixedList.Count; ++i)
			{
				fixedList.Buffer[i].Should().Be(0);
			}
		}

		[Test]
		public void TestTryAdd1()
		{
			var fixedList = new FixedSizeList<int>(4);
			fixedList.Count.Should().Be(0);

			fixedList.TryAdd(4).Should().BeTrue();
			fixedList.Count.Should().Be(1);
			fixedList.Buffer[0].Should().Be(4);
			for (int i = 1; i < fixedList.Count; ++i)
				fixedList.Buffer[i].Should().Be(0);
		}

		[Test]
		public void TestTryAdd2()
		{
			var fixedList = new FixedSizeList<int>(4);
			fixedList.Count.Should().Be(0);
			
			fixedList.TryAdd(42).Should().BeTrue();
			fixedList.TryAdd(9001).Should().BeTrue();
			fixedList.Count.Should().Be(2);
			fixedList.Buffer[0].Should().Be(42);
			fixedList.Buffer[1].Should().Be(9001);
			for (int i = 2; i < fixedList.Count; ++i)
				fixedList.Buffer[i].Should().Be(0);
		}

		[Test]
		public void TestTryAddTooMany()
		{
			var fixedList = new FixedSizeList<int>(3);
			fixedList.Count.Should().Be(0);
			
			fixedList.TryAdd(42).Should().BeTrue();
			fixedList.TryAdd(9001).Should().BeTrue();
			fixedList.TryAdd(10).Should().BeTrue();
			fixedList.TryAdd(1337).Should().BeFalse();
			fixedList.Count.Should().Be(3);
			fixedList.Buffer[0].Should().Be(42);
			fixedList.Buffer[1].Should().Be(9001);
			fixedList.Buffer[2].Should().Be(10);
		}

		[Test]
		public void TestTryAddClearTryAdd()
		{
			var fixedList = new FixedSizeList<int>(3);
			fixedList.Count.Should().Be(0);
			
			fixedList.TryAdd(42).Should().BeTrue();
			fixedList.TryAdd(9001).Should().BeTrue();
			fixedList.TryAdd(10).Should().BeTrue();
			fixedList.TryAdd(1337).Should().BeFalse();
			fixedList.Count.Should().Be(3);
			fixedList.Buffer[0].Should().Be(42);
			fixedList.Buffer[1].Should().Be(9001);
			fixedList.Buffer[2].Should().Be(10);

			fixedList.Clear();
			fixedList.Count.Should().Be(0);

			fixedList.TryAdd(1337).Should().BeTrue();
			fixedList.Count.Should().Be(1);
			fixedList.Buffer[0].Should().Be(1337);
		}
	}
}
