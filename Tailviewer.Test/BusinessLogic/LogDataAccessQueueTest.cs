using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class LogDataAccessQueueTest
	{
		private LogDataAccessQueue<LogEntryIndex, LogEntry> _queue;

		sealed class Accessor
			: ILogDataAccessor<LogEntryIndex, LogEntry>
		{
			private readonly Dictionary<LogEntryIndex, LogEntry> _values;

			public Accessor()
			{
				_values = new Dictionary<LogEntryIndex, LogEntry>();
			}

			public void Add(LogEntryIndex index, LogEntry entry)
			{
				_values.Add(index, entry);
			}

			public bool TryAccess(LogEntryIndex index, out LogEntry data)
			{
				return _values.TryGetValue(index, out data);
			}
		}

		[SetUp]
		public void SetUp()
		{
			_queue = new LogDataAccessQueue<LogEntryIndex, LogEntry>();
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

			var row = new LogEntry("hello", "world");
			var accessor = new Accessor();
			accessor.Add(new LogEntryIndex(42), row);
			_queue.ExecuteOne(accessor);

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

			var accessor = new Accessor();
			accessor.Add(1, new LogEntry("1"));
			accessor.Add(2, new LogEntry("2"));
			accessor.Add(3, new LogEntry("3"));
			_queue.ExecuteAll(accessor);

			task1.IsCompleted.Should().BeTrue();
			task1.Result.Should().Be(new LogEntry("1"));

			task2.IsCompleted.Should().BeTrue();
			task2.Result.Should().Be(new LogEntry("2"));

			task3.IsCompleted.Should().BeTrue();
			task3.Result.Should().Be(new LogEntry("3"));
		}

		[Test]
		[Description("Verifies that it's possible to access the same row multiple times")]
		public void TestEnqueue3()
		{
			var task1 = _queue[new LogEntryIndex(1337)];
			var task2 = _queue[new LogEntryIndex(1337)];
			var task3 = _queue[new LogEntryIndex(1337)];

			var row = new LogEntry("42");
			var accessor = new Accessor();
			accessor.Add(1337, row);
			_queue.ExecuteAll(accessor);

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
			var accessor = new Accessor();
			accessor.Add(1337,row);
			_queue.ExecuteOne(accessor);

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
			var accessor = new Accessor();
			accessor.Add(1337, row);
			_queue.ExecuteOne(accessor);

			task1.IsCompleted.Should().BeTrue();
			task1.Result.Should().Be(row);

			_queue.Count.Should().Be(0);
			var task2 = _queue[new LogEntryIndex(1337)];
			task2.Should().NotBeSameAs(task1, "Because completed/cancelled/faulted tasks may not be reused");
			task2.IsCompleted.Should().BeFalse();

			_queue.ExecuteOne(accessor);
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
			new Action(() => { var unused = task.Result; }).ShouldNotThrow();
			task.Result.Should().Be(default(LogEntry));
		}

		[Test]
		[Description("Verifies that Dispose() can be called multiple times without problems")]
		public void TestDispose3()
		{
			_queue.Dispose();
			new Action(() => _queue.Dispose()).ShouldNotThrow();
			new Action(() => _queue.Dispose()).ShouldNotThrow();
		}
	}
}