using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class LogDataTaskQueueTest
	{
		private LogDataTaskQueue<LogEntryIndex, LogEntry> _queue;

		[SetUp]
		public void SetUp()
		{
			_queue = new LogDataTaskQueue<LogEntryIndex, LogEntry>();
		}

		[Test]
		public void TestCtor()
		{
			_queue.Count.Should().Be(0);
		}

		[Test]
		[Description("Verifies that access to a single row is possible")]
		public void TestEnqueue1()
		{
			var task = _queue[new LogEntryIndex(42)];
			task.Should().NotBeNull();
			task.IsCanceled.Should().BeFalse();
			task.IsCompleted.Should().BeFalse();
			task.IsFaulted.Should().BeFalse();

			_queue.Count.Should().Be(1);

			LogEntryIndex? index = null;
			var row = new LogEntry("hello", "world");
			_queue.ExecuteOne(idx =>
				{
					index = idx;
					return row;
				});

			index.Should().Be(new LogEntryIndex(42), "Because the queue should've tried to access the data for the index we requested");

			task.IsCanceled.Should().BeFalse();
			task.IsFaulted.Should().BeFalse();
			task.IsCompleted.Should().BeTrue();
			task.Result.Should().Be(row, "Because the task should've returned the result passed by the factory");
			_queue.Count.Should().Be(0, "Because the only pending task should've been executed");
		}

		[Test]
		[Description("Verifies that its possible to access different rows before any request is processed")]
		public void TestEnqueue2()
		{
			var task1 = _queue[new LogEntryIndex(1)];
			var task2 = _queue[new LogEntryIndex(2)];
			var task3 = _queue[new LogEntryIndex(3)];

			task1.Should().NotBeNull();

			task2.Should().NotBeNull();
			task2.Should().NotBeSameAs(task1);

			task3.Should().NotBeNull();
			task3.Should().NotBeSameAs(task1);

			var row1 = new LogEntry("1");
			var row2 = new LogEntry("2");
			var row3 = new LogEntry("3");
			_queue.ExecuteAll(index =>
			{
				if (index == 1)
					return row1;
				if (index == 2)
					return row2;
				if (index == 3)
					return row3;
				return new LogEntry();
			});

			task1.IsCompleted.Should().BeTrue();
			task1.Result.Should().Be(row1);

			task2.IsCompleted.Should().BeTrue();
			task2.Result.Should().Be(row2);

			task3.IsCompleted.Should().BeTrue();
			task3.Result.Should().Be(row3);
		}

		[Test]
		[Description("Verifies that it's possible to access the same row multiple times")]
		public void TestEnqueue3()
		{
			var task1 = _queue[new LogEntryIndex(1337)];
			var task2 = _queue[new LogEntryIndex(1337)];
			var task3 = _queue[new LogEntryIndex(1337)];

			var row = new LogEntry("42");
			_queue.ExecuteAll(index =>
				{
					if (index == 1337)
						return row;
					return new LogEntry();
				});

			task1.IsCompleted.Should().BeTrue();
			task1.Result.Should().Be(row);

			task2.IsCompleted.Should().BeTrue();
			task2.Result.Should().Be(row);

			task3.IsCompleted.Should().BeTrue();
			task3.Result.Should().Be(row);
		}

		[Test]
		[Description("Verifies that access to the same row is optimized to the same task")]
		public void TestEnqueue4()
		{
			var task1 = _queue[new LogEntryIndex(1337)];
			var task2 = _queue[new LogEntryIndex(1337)];
			task2.Should().BeSameAs(task1);

			var task3 = _queue[new LogEntryIndex(1337)];
			task3.Should().BeSameAs(task1);

			_queue.Count.Should().Be(1, "Because multiple accesses to the same row shall be optimized to one single access");
			
			var row = new LogEntry("42");
			_queue.ExecuteOne(index =>
				{
					if (index == 1337)
						return row;

					return new LogEntry();
				});

			task1.IsCompleted.Should().BeTrue();
			task1.Result.Should().Be(row);

			task2.IsCompleted.Should().BeTrue();
			task2.Result.Should().Be(row);

			task3.IsCompleted.Should().BeTrue();
			task3.Result.Should().Be(row);
		}

		[Test]
		[Description("Verifies that completed tasks aren't reused (specifically we test that the optimization properly removes finished tasks from its dictionary)")]
		public void TestEnqueue5()
		{
			var task1 = _queue[new LogEntryIndex(1337)];
			var row = new LogEntry("42");
			_queue.ExecuteOne(index => row);

			task1.IsCompleted.Should().BeTrue();
			task1.Result.Should().Be(row);

			_queue.Count.Should().Be(0);
			var task2 = _queue[new LogEntryIndex(1337)];
			task2.Should().NotBeSameAs(task1, "Because completed/cancelled/faulted tasks may not be reused");
			task2.IsCompleted.Should().BeFalse();

			_queue.ExecuteOne(index => row);
			task2.IsCompleted.Should().BeTrue();
			task2.Result.Should().Be(row);
		}

		[Test]
		[Description("Verifies that an empty queue can be disposed of")]
		public void TestDispose1()
		{
			new Action(() => _queue.Dispose()).ShouldNotThrow();
		}

		[Test]
		[Description("Verifies that pending tasks are cancelled when the queue is disposed of")]
		public void TestDispose2()
		{
			var task = _queue[new LogEntryIndex(42)];
			new Action(() => _queue.Dispose()).ShouldNotThrow();
			task.IsCanceled.Should().BeTrue();
			new Action(() => { var unused = task.Result; }).ShouldThrow<TaskCanceledException>();
		}
	}
}