using System;
using System.Collections.Generic;
using System.Threading;
using Moq;
using NUnit.Framework;
using Tailviewer.Core.Buffers;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Sources.Buffer;

namespace Tailviewer.Test.BusinessLogic.Sources.Buffer
{
	[TestFixture]
	public sealed class BufferedLogSourceTest2
	{
		private ManualTaskScheduler _taskScheduler;
		private Mock<ILogSource> _source;
		private LogBufferList _data;

		[SetUp]
		public void Setup()
		{
			_taskScheduler = new ManualTaskScheduler();
			_source = new Mock<ILogSource>();
			_source.Setup(x => x.Columns).Returns(new[] {GeneralColumns.RawContent});
		}

		[Test]
		[Description("Verifies that the buffer retrieves the data from the source when it's not cached and we allowed it to do so")]
		public void TestFetchFromSourceWhenNotCached()
		{
			var buffer = new BufferedLogSource(_taskScheduler, _source.Object, TimeSpan.Zero);
			var destination = new LogBufferArray(4, new IColumnDescriptor[] {GeneralColumns.Index, GeneralColumns.RawContent});
			var queryOptions = new LogSourceQueryOptions(LogSourceQueryMode.FromCache | LogSourceQueryMode.FromSource);
			buffer.GetEntries(new LogFileSection(10, 4), destination, 0, queryOptions);
			_source.Verify(x => x.GetEntries(new LogFileSection(10, 4), destination, 0, queryOptions),
			               Times.Once);
		}

		[Test]
		[Description("Verifies that the buffer retrieves the data from the source when we don't allow it to retrieve the data from the cache")]
		public void TestFetchFromSourceWhenNotAllowedFromCache()
		{
			var buffer = new BufferedLogSource(_taskScheduler, _source.Object, TimeSpan.Zero);
			var destination = new LogBufferArray(4, new IColumnDescriptor[] {GeneralColumns.Index, GeneralColumns.RawContent});
			var queryOptions = new LogSourceQueryOptions(LogSourceQueryMode.FromSource);
			buffer.GetEntries(new LogFileSection(10, 4), destination, 0, queryOptions);
			_source.Verify(x => x.GetEntries(new LogFileSection(10, 4), destination, 0, queryOptions),
			               Times.Once);
		}

		[Test]
		[Description("Verifies that the buffer does not retrieve the data from the source when this has been specified via the query mode")]
		public void TestSkipSourceIfNotAllowed()
		{
			var buffer = new BufferedLogSource(_taskScheduler, _source.Object, TimeSpan.Zero);
			var destination = new LogBufferArray(4, new IColumnDescriptor[] {GeneralColumns.Index, GeneralColumns.RawContent});
			var queryOptions = new LogSourceQueryOptions(LogSourceQueryMode.FromCache);
			buffer.GetEntries(new LogFileSection(10, 4), destination, 0, queryOptions);
			_source.Verify(x => x.GetEntries(It.IsAny<IReadOnlyList<LogLineIndex>>(), It.IsAny<ILogBuffer>(), It.IsAny<int>(), It.IsAny<LogSourceQueryOptions>()),
			               Times.Never, "because we didn't allow the data to be retrieved from the source under any circumstances");
		}

		[Test]
		[Description("Verifies that the buffer tries to prefetch the entire page that somebody tried to access")]
		public void TestPrefetchAsync([Values(10, 100, 1000)] int pageSize)
		{
			var buffer = new BufferedLogSource(_taskScheduler, _source.Object, TimeSpan.Zero, pageSize: pageSize);
			var destination = new LogBufferArray(4, new IColumnDescriptor[] {GeneralColumns.Index, GeneralColumns.RawContent});
			var queryOptions = new LogSourceQueryOptions(LogSourceQueryMode.FromCache | LogSourceQueryMode.FetchForLater);

			buffer.OnLogFileModified(_source.Object, new LogFileSection(0, 10));

			var sectionToQuery = new LogFileSection(2, 4);
			buffer.GetEntries(sectionToQuery, destination, 0, queryOptions);
			_source.Verify(x => x.GetEntries(It.IsAny<IReadOnlyList<LogLineIndex>>(), It.IsAny<ILogBuffer>(), It.IsAny<int>(), It.IsAny<LogSourceQueryOptions>()),
			               Times.Never, "Because we didn't allow data to be retrieved on the calling thread");

			_taskScheduler.RunOnce();
			_source.Verify(x => x.GetEntries(new LogFileSection(0, pageSize), It.IsAny<ILogBuffer>(), It.IsAny<int>(), It.IsAny<LogSourceQueryOptions>()),
			               Times.Once, "Because the buffer should have tried to retrieve the page for the entire page which was accessed");
		}
	}
}
