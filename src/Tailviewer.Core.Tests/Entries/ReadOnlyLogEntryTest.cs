using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Entries
{
	[TestFixture]
	public sealed class ReadOnlyLogEntryTest
		: AbstractReadOnlyLogEntryTest
	{
		protected override IReadOnlyLogEntry CreateDefault()
		{
			return new ReadOnlyLogEntry();
		}

		protected override IReadOnlyLogEntry CreateEmpty()
		{
			return new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>());
		}

		[Test]
		public void TestEqualBothEmpty()
		{
			var entry = new ReadOnlyLogEntry();
			var equalEntry = new ReadOnlyLogEntry();
			Equals(entry, equalEntry).Should().BeTrue();

			var equalReadOnlyEntry = new ReadOnlyLogEntry();
			Equals(entry, equalReadOnlyEntry).Should().BeTrue();
		}

		[Test]
		public void TestEqualSameValue()
		{
			var values = new Dictionary<IColumnDescriptor, object>
			{
				{Core.Columns.RawContent, "Starbuck"}
			};
			var entry = new LogEntry(values);
			var equalEntry = new LogEntry(values);
			Equals(entry, equalEntry).Should().BeTrue();

			var equalReadOnlyEntry = new ReadOnlyLogEntry(values);
			Equals(entry, equalReadOnlyEntry).Should().BeTrue();
		}

		[Test]
		public void TestEqualDifferentValue()
		{
			var values = new Dictionary<IColumnDescriptor, object>
			{
				{Core.Columns.RawContent, "Starbuck"}
			};
			var otherValues = new Dictionary<IColumnDescriptor, object>
			{
				{Core.Columns.RawContent, "Apollo"}
			};
			var entry = new LogEntry(values);
			var otherEntry = new LogEntry(otherValues);
			Equals(entry, otherEntry).Should().BeFalse();

			var otherReadOnlyEntry = new ReadOnlyLogEntry(otherValues);
			Equals(entry, otherReadOnlyEntry).Should().BeFalse();
		}

		[Test]
		public void TestEqualBothEmpty_DifferentColumns()
		{
			var entry = new LogEntry(Core.Columns.RawContent);
			var otherEntry = new LogEntry(Core.Columns.RawContent, Core.Columns.Timestamp);
			Equals(entry, otherEntry).Should().BeFalse();

			var equalReadOnlyEntry = new ReadOnlyLogEntry(Core.Columns.RawContent, Core.Columns.Timestamp);
			Equals(entry, equalReadOnlyEntry).Should().BeFalse();
		}
	}
}