using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core;
using Tailviewer.Core.LogTables;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class LogDataCacheTest
	{
		private LogDataCache _cache;
		private ILogTable _table1;
		private ILogFile _file1;
		private ILogTable _table2;
		private ILogFile _file2;

		[SetUp]
		public void SetUp()
		{
			_cache = new LogDataCache();

			_table1 = new Mock<ILogTable>().Object;
			_table2 = new Mock<ILogTable>().Object;
			_file1 = new Mock<ILogFile>().Object;
			_file2 = new Mock<ILogFile>().Object;
		}

		[Test]
		public void TestCtor()
		{
			_cache.Size.Should().Be(Size.Zero);
		}

		[Test]
		[Description("")]
		public void TestTryGet1()
		{
			LogLine line;
			_cache.TryGetValue(_file1, 42, out line).Should().BeFalse("because the cache is empty");

			LogEntry entry;
			_cache.TryGetValue(_table1, 42, out entry).Should().BeFalse("because the cache is empty");
		}

		[Test]
		public void TestTryGet2()
		{
			var line = new LogLine(42, 42, "hello World!", LevelFlags.None);
			_cache.Add(_file1, 42, line);

			LogLine actualLine;
			_cache.TryGetValue(_file1, 42, out actualLine).Should().BeTrue();
			actualLine.Should().Be(line);

			_cache.TryGetValue(_file2, 42, out actualLine).Should().BeFalse();
			actualLine.Should().Be(default(LogLine));

			_cache.TryGetValue(_file1, 41, out actualLine).Should().BeFalse();
			actualLine.Should().Be(default(LogLine));
		}

		[Test]
		public void TestContains1()
		{
			_cache.Contains(null, LogLineIndex.Invalid).Should().BeFalse();
			_cache.Contains(_file1, LogLineIndex.Invalid).Should().BeFalse();
			_cache.Contains(null, LogEntryIndex.Invalid).Should().BeFalse();
			_cache.Contains(_table1, LogEntryIndex.Invalid).Should().BeFalse();
		}

		[Test]
		public void TestAdd1()
		{
			_cache.Count.Should().Be(0);
			_cache.Add(_file1, 42, new LogLine());
			_cache.Count.Should().Be(1);

			_cache.Add(_table1, 42, new LogEntry());
			_cache.Count.Should().Be(2);
		}

		[Test]
		[Description("Verifies that two lines at the same index but from different sources can be added")]
		public void TestAdd2()
		{
			_cache.Add(_file1, 1337, new LogLine());
			_cache.Add(_file2, 1337, new LogLine());

			_cache.Contains(_file1, 1337).Should().BeTrue();
			_cache.Contains(_file2, 1337).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that two entries at the same index but from different sources can be added")]
		public void TestAdd3()
		{
			_cache.Add(_table1, 1337, new LogEntry());
			_cache.Add(_table2, 1337, new LogEntry());

			_cache.Contains(_table1, 1337).Should().BeTrue();
			_cache.Contains(_table2, 1337).Should().BeTrue();
		}

		[Test]
		public void TestAdd4()
		{
			for (int i = 0; i < 10000; ++i)
			{
				_cache.Add(_file1, i, new LogLine());
			}

			_cache.Count.Should().Be(10000);
			_cache.Remove(_file1);
			_cache.Count.Should().Be(0);
		}

		[Test]
		public void TestAdd5()
		{
			for (int i = 0; i < 10000; ++i)
			{
				_cache.Add(_table1, i, new LogEntry());
			}

			_cache.Count.Should().Be(10000);
			_cache.Remove(_table1);
			_cache.Count.Should().Be(0);
		}
	}
}