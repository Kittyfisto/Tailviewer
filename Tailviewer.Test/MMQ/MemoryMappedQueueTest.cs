using System;
using System.IO;
using System.IO.MemoryMappedFiles;
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
		public void TestCreateQueue1()
		{
			using (var queue = MemoryMappedQueue.Create("Foobar"))
			{
				queue.Should().NotBeNull();

				new Action(() => MemoryMappedFile.CreateNew("Foobar", 123))
					.ShouldThrow<IOException>("because Create() is supposed to create an underlying memor mapped file");
			}
		}

		[Test]
		public void TestCreateProducer1()
		{
			using (var queue = MemoryMappedQueue.Create("Foo"))
			using (var producer = queue.CreateProducer())
			{
				producer.Should().NotBeNull();
			}
		}

		[Test]
		public void TestCreateConsumer1()
		{
			using (var queue = MemoryMappedQueue.Create("Foo"))
			using (var consumer = queue.CreateConsumer())
			{
				consumer.Should().NotBeNull();
			}
		}

		[Test]
		[Description("Verifies that simple messages can be exchanged")]
		public void TestEnqueueDequeue1([Values(new byte[] {1},
												new byte[] {42, 99},
												new byte[] {200, 91, 12},
												new byte[] {0, 1, 44, 125, 231, 200, 100})] byte[] message)
		{
			using (var queue = MemoryMappedQueue.Create("Test1"))
			using (var producer = MemoryMappedQueue.CreateProducer("Test1"))
			using (var consumer = queue.CreateConsumer())
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
			using (var producer = queue.CreateProducer())
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
			using (var consumer = queue.CreateConsumer())
			{
				new Action(() => consumer.Dequeue(TimeSpan.Zero)).ShouldThrow<TimeoutException>();
				byte[] unused;
				consumer.TryDequeue(out unused, TimeSpan.Zero).Should().BeFalse();
			}
		}
	}
}