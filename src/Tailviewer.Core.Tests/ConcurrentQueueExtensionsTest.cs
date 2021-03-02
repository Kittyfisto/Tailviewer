using System.Collections.Concurrent;
using FluentAssertions;
using NUnit.Framework;

namespace Tailviewer.Core.Tests
{
	[TestFixture]
	public sealed class ConcurrentQueueExtensionsTest
	{
		[Test]
		public void TestDequeueAllEmpty()
		{
			var queue = new ConcurrentQueue<int>(new int[0]);
			var values = queue.DequeueAll();
			values.Should().NotBeNull();
			values.Should().BeEmpty();
			values.Should().BeSameAs(EnumerableExtensions<int>.Empty, "because the method shouldn't allocate memory unless necessary");
		}

		[Test]
		public void TestDequeueAllElementsInOrder()
		{
			var queue = new ConcurrentQueue<int>(new []{1, 42, 9, 8});
			var values = queue.DequeueAll();
			values.Should().NotBeNull();
			values.Should().Equal(new[] {1, 42, 9, 8});
			queue.Should().BeEmpty("because the method should have actually dequeued the elements from the queue");
		}
	}
}
