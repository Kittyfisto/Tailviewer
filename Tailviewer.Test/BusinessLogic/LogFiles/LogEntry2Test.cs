using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogEntry2Test
	{
		[Test]
		public void TestConstruction()
		{
			var entry = new LogEntry2();
			entry.Columns.Should().BeEmpty();
			new Action(() => { var unused = entry.DeltaTime; }).ShouldThrow<NoSuchColumnException>();
			new Action(() => { var unused = entry.ElapsedTime; }).ShouldThrow<NoSuchColumnException>();
			new Action(() => { var unused = entry.Index; }).ShouldThrow<NoSuchColumnException>();
			new Action(() => { var unused = entry.LineNumber; }).ShouldThrow<NoSuchColumnException>();
			new Action(() => { var unused = entry.LogEntryIndex; }).ShouldThrow<NoSuchColumnException>();
			new Action(() => { var unused = entry.LogLevel; }).ShouldThrow<NoSuchColumnException>();
			new Action(() => { var unused = entry.OriginalIndex; }).ShouldThrow<NoSuchColumnException>();
			new Action(() => { var unused = entry.OriginalLineNumber; }).ShouldThrow<NoSuchColumnException>();
			new Action(() => { var unused = entry.RawContent; }).ShouldThrow<NoSuchColumnException>();
			new Action(() => { var unused = entry.Timestamp; }).ShouldThrow<NoSuchColumnException>();
		}
	}
}