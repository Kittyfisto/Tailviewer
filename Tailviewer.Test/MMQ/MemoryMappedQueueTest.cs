using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.MMQ;

namespace Tailviewer.Test.MMQ
{
	[TestFixture]
	public sealed class MemoryMappedQueueTest
	{
		[Test]
		[Description("Verifies that simple messages can be exchanged")]
		public void TestEnqueueDequeue1([Values(new byte[] {1},
												new byte[] {42, 99},
												new byte[] {200, 91, 12},
												new byte[] {0, 1, 44, 125, 231, 200, 100})] byte[] message)
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

		[Test]
		public void TestEnqueueDequeueMany()
		{
			using (var queue = MemoryMappedQueue.Create("Test1"))
			using (var producer = MemoryMappedQueue.CreateProducer("Test1"))
			using (var consumer = MemoryMappedQueue.CreateConsumer("Test1"))
			{
				const int messageLength = 512;
				const int numMessages = 1000;

				for(int i = 0; i < numMessages; ++i)
				{
					var message = Enumerable.Range(0, messageLength).Select(n => (byte) (n % 255)).ToArray();

					producer.Enqueue(message);
					var actualMessage = consumer.Dequeue();
					actualMessage.Should().Equal(message);
				}
			}
		}

		[Test]
		[Description("Verifies that dequeueing an empty queue results in a timeout")]
		public void TestDequeue1()
		{
			using (var queue = MemoryMappedQueue.Create("Test1"))
			using (var consumer = MemoryMappedQueue.CreateConsumer("Test1"))
			{
				new Action(() => consumer.Dequeue(TimeSpan.Zero)).ShouldThrow<TimeoutException>();
				byte[] unused;
				consumer.TryDequeue(out unused, TimeSpan.Zero).Should().BeFalse();
			}
		}
	}
}