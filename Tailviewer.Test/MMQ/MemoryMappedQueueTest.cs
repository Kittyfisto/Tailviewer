using FluentAssertions;
using NUnit.Framework;
using Tailviewer.MMQ;

namespace Tailviewer.Test.MMQ
{
	[TestFixture]
	public sealed class MemoryMappedQueueTest
	{
		[Test]
		[Ignore("Not yet working")]
		public void TestEnqueueDequeue1([Values(new byte[] {1},
												new byte[] {42, 99},
												new byte[] {200, 91, 12})] byte[] message)
		{
			using (var queue = MemoryMappedQueue.Create("Test1"))
			using (var producer = MemoryMappedQueue.CreateProducer("Test1"))
			using (var consumer = MemoryMappedQueue.CreateConsumer("Test1"))
			{
				producer.Enqueue(message);
				var actualMessage = consumer.Dequeue();

				actualMessage.Should().NotBeNull();
				actualMessage.Should().NotBeSameAs(message);
				actualMessage.Should().Equal(message);
			}
		}
	}
}