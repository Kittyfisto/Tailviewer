﻿using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Sources.Adorner
{
	[TestFixture]
	public sealed class LogSourceColumnAdornerTest
	{
		[Test]
		public void TestElapsedTime()
		{
			var source = new InMemoryLogSource();
			source.Add(new LogEntry {Timestamp = new DateTime(2021, 02, 20, 16, 45, 01)});
			source.Add(new LogEntry {Timestamp = new DateTime(2021, 02, 20, 16, 46, 02)});
			source.Add(new LogEntry {Timestamp = new DateTime(2021, 02, 20, 16, 46, 05)});

			var adorner = new LogSourceColumnAdorner(source);
			var entries = adorner.GetEntries(new IColumnDescriptor[] {Core.Columns.ElapsedTime});
			entries.Should().HaveCount(3);
			entries[0].ElapsedTime.Should().Be(TimeSpan.Zero);
			entries[1].ElapsedTime.Should().Be(TimeSpan.FromSeconds(61));
			entries[2].ElapsedTime.Should().Be(TimeSpan.FromSeconds(64));
		}

		[Test]
		public void TestDeltaTime()
		{
			var source = new InMemoryLogSource();
			source.Add(new LogEntry {Timestamp = new DateTime(2021, 02, 20, 16, 45, 01)});
			source.Add(new LogEntry {Timestamp = new DateTime(2021, 02, 20, 16, 46, 02)});
			source.Add(new LogEntry {Timestamp = new DateTime(2021, 02, 20, 16, 46, 05)});

			var adorner = new LogSourceColumnAdorner(source);
			var entries = adorner.GetEntries(new IColumnDescriptor[] {Core.Columns.DeltaTime});
			entries.Should().HaveCount(3);
			entries[0].DeltaTime.Should().Be(null);
			entries[1].DeltaTime.Should().Be(TimeSpan.FromSeconds(61));
			entries[2].DeltaTime.Should().Be(TimeSpan.FromSeconds(3));
		}
	}
}
