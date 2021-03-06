﻿using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Api.Tests;

namespace Tailviewer.Core.Tests.Sources.MultiLine
{
	[TestFixture]
	public sealed class MultiLineLogSourceTest
		: AbstractLogSourceTest
	{
		[SetUp]
		public void Setup()
		{
			_taskScheduler = new ManualTaskScheduler();

			_entries = new LogBufferList();
			_source = new Mock<ILogSource>();
			_source.Setup(x => x.GetProperty(Core.Properties.LogEntryCount)).Returns(() => _entries.Count);
			_source.Setup(x => x.AddListener(It.IsAny<ILogSourceListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
				.Callback((ILogSourceListener listener, TimeSpan unused1, int unused2) =>
				{
					listener.OnLogFileModified(_source.Object,
						LogSourceModification.Reset());
				});

			_modifications = new List<LogSourceModification>();
			_listener = new Mock<ILogSourceListener>();
			_listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogSourceModification>()))
					 .Callback((ILogSource l, LogSourceModification s) => _modifications.Add(s));

		}

		private Mock<ILogSource> _source;
		private ManualTaskScheduler _taskScheduler;
		private List<LogSourceModification> _modifications;
		private Mock<ILogSourceListener> _listener;
		private LogBufferList _entries;

		[Test]
		public void TestCtor1()
		{
			var logFile = new MultiLineLogSource(_taskScheduler, _source.Object, TimeSpan.Zero);
			_taskScheduler.PeriodicTaskCount.Should().Be(1);

			logFile.Dispose();
			_taskScheduler.PeriodicTaskCount.Should().Be(0);
		}

		[Test]
		public void TestCtor2()
		{
			var logFile = new MultiLineLogSource(_taskScheduler, _source.Object, TimeSpan.Zero);
			_source.Verify(
				x => x.AddListener(It.Is<ILogSourceListener>(y => Equals(y, logFile)), It.IsAny<TimeSpan>(), It.IsAny<int>()),
				Times.Once,
				"because the single line log file should register itself as a source at the listener");

			new Action(() => logFile.Dispose()).Should().NotThrow("because Dispose() must always succeed");
			_source.Verify(x => x.RemoveListener(It.Is<ILogSourceListener>(y => Equals(y, logFile))), Times.Once,
				"because the single line log file should remove itself as a listener from its source upon being disposed of");
		}

		[Test]
		[Description("Verifies that MaxCharactersPerLine is changed once a modification is applied")]
		public void TestOneModification2()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			_taskScheduler.RunOnce();
			logFile.GetProperty(TextProperties.MaxCharactersInLine).Should().Be(0);

			source.AddEntry("Hi, I’m new to driving and I need to move ");
			logFile.GetProperty(TextProperties.MaxCharactersInLine).Should().Be(0, "because the change shouldn't have been applied yet");

			_taskScheduler.RunOnce();
			logFile.GetProperty(TextProperties.MaxCharactersInLine).Should().Be(42, "because the change should have been applied by now");
		}

		[Test]
		[Description("Verifies that the Exists flag is changed once a modification is applied")]
		public void TestOneModification3()
		{
			var emptyReason = new Mock<IEmptyReason>();
			_source.Setup(x => x.GetProperty(Core.Properties.EmptyReason)).Returns(emptyReason.Object);
			_source.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
			       .Callback((IPropertiesBuffer destination) =>
				                 destination.SetValue(Core.Properties.EmptyReason, emptyReason.Object));
			var logFile = new MultiLineLogSource(_taskScheduler, _source.Object, TimeSpan.Zero);
			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.EmptyReason).Should().Be(emptyReason.Object, "because the source doesn't exist (yet)");

			_entries.Add(new LogEntry());
			_source.Setup(x => x.GetProperty(Core.Properties.EmptyReason)).Returns((IEmptyReason) null);
			_source.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
			       .Callback((IPropertiesBuffer properties) =>
			       {
				       properties.SetValue(Core.Properties.EmptyReason, null);
			       });

			logFile.GetProperty(Core.Properties.EmptyReason).Should().Be(emptyReason.Object, "because the change shouldn't have been applied yet");

			logFile.OnLogFileModified(_source.Object, LogSourceModification.Appended(0, 1));
			logFile.GetProperty(Core.Properties.EmptyReason).Should().Be(emptyReason.Object, "because the change shouldn't have been applied yet");

			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.EmptyReason).Should().Be(null, "because the change should have been applied by now");
		}

		[Test]
		[Description("Verifies that the StartTimestamp is changed once a modification is applied")]
		public void TestOneModification4()
		{
			var logFile = new MultiLineLogSource(_taskScheduler, _source.Object, TimeSpan.Zero);
			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.StartTimestamp).Should().NotHaveValue("because the source doesn't exist (yet)");

			var timestamp = new DateTime(2017, 3, 15, 22, 40, 0);
			_entries.Add(new LogEntry());
			_source.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
			       .Callback((IPropertiesBuffer properties) =>
			       {
				       properties.SetValue(Core.Properties.StartTimestamp, timestamp);
			       });
			logFile.GetProperty(Core.Properties.StartTimestamp).Should().NotHaveValue("because the change shouldn't have been applied yet");

			logFile.OnLogFileModified(_source.Object, LogSourceModification.Appended(0, 1));
			logFile.GetProperty(Core.Properties.StartTimestamp).Should().NotHaveValue("because the change shouldn't have been applied yet");

			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.StartTimestamp).Should().Be(timestamp, "because the change should have been applied by now");
		}

		[Test]
		[Description("Verifies that the LastModified is changed once a modification is applied")]
		public void TestOneModification5()
		{
			var logFile = new MultiLineLogSource(_taskScheduler, _source.Object, TimeSpan.Zero);
			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.LastModified).Should().NotHaveValue("because the source doesn't exist (yet)");

			var timestamp = new DateTime(2017, 3, 15, 22, 40, 0);
			_entries.Add(new LogEntry());
			_source.Setup(x => x.GetProperty(Core.Properties.LastModified)).Returns(timestamp);
			_source.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
			       .Callback((IPropertiesBuffer properties) =>
			       {
				       properties.SetValue(Core.Properties.LastModified, timestamp);
			       });

			logFile.GetProperty(Core.Properties.LastModified).Should().NotHaveValue("because the change shouldn't have been applied yet");

			logFile.OnLogFileModified(_source.Object, LogSourceModification.Appended(0, 1));
			logFile.GetProperty(Core.Properties.LastModified).Should().NotHaveValue("because the change shouldn't have been applied yet");

			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.LastModified).Should().Be(timestamp, "because the change should have been applied by now");
		}

		[Test]
		[Description("Verifies that the Size is changed once a modification is applied")]
		public void TestOneModification6()
		{
			var logFile = new MultiLineLogSource(_taskScheduler, _source.Object, TimeSpan.Zero);
			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.Size).Should().BeNull("because the source doesn't exist (yet)");

			var size = Size.FromGigabytes(42);
			_entries.Add(new LogEntry());
			_source.Setup(x => x.GetProperty(Core.Properties.Size)).Returns(size);
			_source.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
			       .Callback((IPropertiesBuffer properties) =>
			       {
					   properties.SetValue(Core.Properties.Size, size);
			       });

			logFile.GetProperty(Core.Properties.Size).Should().BeNull("because the change shouldn't have been applied yet");

			logFile.OnLogFileModified(_source.Object, LogSourceModification.Appended(0, 1));
			logFile.GetProperty(Core.Properties.Size).Should().BeNull("because the change shouldn't have been applied yet");

			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.Size).Should().Be(size, "because the change should have been applied by now");
		}

		[Test]
		[Description("Verifies that receiving a Reset() actually causes the entire content to be reset")]
		public void TestReset1()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			source.AddEntry("INFO: hello", LevelFlags.Info);
			_taskScheduler.RunOnce();

			_entries.Clear();
			logFile.OnLogFileModified(_source.Object, LogSourceModification.Reset());
			_taskScheduler.RunOnce();

			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(0, "because the source is completely empty");
		}

		[Test]
		[Description("Verifies that the log file can represent completely new content after reset")]
		public void TestReset2()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			source.AddEntry("A", LevelFlags.Info);
			source.AddEntry("B", LevelFlags.Warning);
			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(2);

			source.Clear();
			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(0);

			source.AddEntry("A", LevelFlags.Info);
			source.AddEntry("A continued", LevelFlags.Other);
			_taskScheduler.RunOnce();

			var entries = logFile.GetEntries(new LogSourceSection(0, 2));
			entries[0].Index.Should().Be(0);
			entries[0].LogEntryIndex.Should().Be(0);
			entries[0].RawContent.Should().Be("A");
			entries[0].LogLevel.Should().Be(LevelFlags.Info);
			entries[1].Index.Should().Be(1);
			entries[1].RawContent.Should().Be("A continued");
			entries[1].LogLevel.Should().Be(LevelFlags.Info);
			entries[1].LogEntryIndex.Should().Be(0, "because this line belongs to the previous one, forming a single entry");
		}

		[Test]
		[Description("Verifies that receiving a Reset() actually causes the Reset() to be forwarded to all listeners")]
		public void TestReset3()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			logFile.AddListener(_listener.Object, TimeSpan.Zero, 10);

			source.AddEntry("INFO: hello", LevelFlags.Info);
			_taskScheduler.RunOnce();

			source.Clear();
			_taskScheduler.RunOnce();

			_modifications.Should().Equal(new object[]
			{
				LogSourceModification.Reset(),
				LogSourceModification.Appended(0, 1),
				LogSourceModification.Reset()
			});
		}

		[Test]
		public void TestOneLine1()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);

			source.AddEntry("INFO: Hello ", LevelFlags.Info);
			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(1);
			var entry = logFile.GetEntry(0);
			entry.Index.Should().Be(0);
			entry.LogEntryIndex.Should().Be(0);
			entry.RawContent.Should().Be("INFO: Hello ");
			entry.LogLevel.Should().Be(LevelFlags.Info);

			source.RemoveFrom(0);
			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(0);

			source.AddEntry("INFO: Hello World!", LevelFlags.Info);
			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(1);
			entry = logFile.GetEntry(0);
			entry.Index.Should().Be(0);
			entry.LogEntryIndex.Should().Be(0);
			entry.RawContent.Should().Be("INFO: Hello World!");
			entry.LogLevel.Should().Be(LevelFlags.Info);
		}

		[Test]
		public void TestOneLine2()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			logFile.AddListener(_listener.Object, TimeSpan.Zero, 10);

			source.AddEntry("INFO: Hello ", LevelFlags.Info);
			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(1);
			_modifications.Should().Equal(new object[] {LogSourceModification.Reset(), LogSourceModification.Appended(0, 1)});

			source.RemoveFrom(0);
			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(0);

			source.AddEntry("Hello World!", LevelFlags.Other);
			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(1);
			_modifications.Should().Equal(new object[]
			{
				LogSourceModification.Reset(),
				LogSourceModification.Appended(0, 1),
				LogSourceModification.Removed(0, 1),
				LogSourceModification.Appended(0, 1)
			});

			var entry = logFile.GetEntry(0);
			entry.Index.Should().Be(0);
			entry.LogEntryIndex.Should().Be(0);
			entry.RawContent.Should().Be("Hello World!");
			entry.LogLevel.Should().Be(LevelFlags.Other);
		}

		[Test]
		public void TestOneLine3()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);

			source.AddEntry("Hello World!", LevelFlags.Other);
			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(1);
			var entry = logFile.GetEntry(0);
			entry.Index.Should().Be(0);
			entry.LogEntryIndex.Should().Be(0);
			entry.RawContent.Should().Be("Hello World!");
			entry.LogLevel.Should().Be(LevelFlags.Other);
		}

		[Test]
		public void TestOneEntry1()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);

			var timestamp = new DateTime(2017, 3, 15, 21, 52, 0);
			source.AddRange(new[]
			{
				new LogEntry{RawContent = "INFO: hello", LogLevel = LevelFlags.Info, Timestamp = timestamp},
				new LogEntry{RawContent = "world!", LogLevel = LevelFlags.Other}
			});

			_taskScheduler.RunOnce();

			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(2);
			var entries = logFile.GetEntries(new LogSourceSection(0, 2));
			entries[0].Index.Should().Be(0);
			entries[0].LogEntryIndex.Should().Be(0);
			entries[0].RawContent.Should().Be("INFO: hello");
			entries[0].LogLevel.Should().Be(LevelFlags.Info);
			entries[1].Index.Should().Be(1);
			entries[1].LogEntryIndex.Should().Be(0, "because this line belongs to the previous one, forming a single entry");
			entries[1].RawContent.Should().Be("world!");
			entries[1].LogLevel.Should().Be(LevelFlags.Info);
		}

		[Test]
		[Description("Verifies that the log file correctly assembles a log event that arrives as two separate lines")]
		public void TestOneEntry2()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);

			var timestamp = new DateTime(2017, 3, 15, 21, 52, 0);
			source.AddEntry("hello", LevelFlags.Info, timestamp);
			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(1);

			source.AddEntry("world!", LevelFlags.Other);
			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(2);
			var entries = logFile.GetEntries(new LogSourceSection(0, 2));
			entries[0].Index.Should().Be(0);
			entries[0].LogEntryIndex.Should().Be(0);
			entries[0].RawContent.Should().Be("hello");
			entries[0].LogLevel.Should().Be(LevelFlags.Info);
			entries[1].Index.Should().Be(1);
			entries[1].LogEntryIndex.Should().Be(0, "because this line belongs to the previous one, forming a single entry");
			entries[1].RawContent.Should().Be("world!");
			entries[1].LogLevel.Should().Be(LevelFlags.Info);
		}

		[Test]
		[Description("Verifies that the log file correctly fires invalidation events to its listeners when a log entry arrives in multiple parts")]
		public void TestOneEntry3()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			logFile.AddListener(_listener.Object, TimeSpan.Zero, 10);

			source.AddEntry("INFO: hello", LevelFlags.Info);
			_taskScheduler.RunOnce();
			_modifications.Should().Equal(new object[] {LogSourceModification.Reset(), LogSourceModification.Appended(0, 1)});

			_modifications.Clear();
			source.AddEntry("world!", LevelFlags.Other);
			_taskScheduler.RunOnce();
			_modifications.Should().Equal(new object[]
			{
				LogSourceModification.Appended(1, 1)
			});
		}

		[Test]
		[Description("Verifies that the log file correctly fires invalidation events to its listeners when a log entry arrives in multiple parts")]
		public void TestTwoEntries1()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);

			source.AddEntry("DEBUG: Starting...", LevelFlags.Debug);
			_taskScheduler.RunOnce();

			logFile.AddListener(_listener.Object, TimeSpan.Zero, 10);
			source.AddEntry("INFO: hello", LevelFlags.Info);
			_taskScheduler.RunOnce();
			_modifications.Should().Equal(new object[]
			{
				LogSourceModification.Reset(),
				LogSourceModification.Appended(0, 1),
				LogSourceModification.Appended(1, 1)
			});

			_modifications.Clear();
			source.AddEntry("world!", LevelFlags.Other);
			_taskScheduler.RunOnce();
			_modifications.Should().Equal(new object[]
			{
				LogSourceModification.Appended(2, 1)
			});
		}

		[Test]
		[Description("Verifies that the log file correctly interprets many single line log entries")]
		public void TestManyEntries1()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			source.AddRange(new[]
			{
				new LogEntry {RawContent = "A", LogLevel = LevelFlags.Debug},
				new LogEntry {RawContent = "B", LogLevel = LevelFlags.Info},
				new LogEntry {RawContent = "C", LogLevel = LevelFlags.Warning},
				new LogEntry {RawContent = "D", LogLevel = LevelFlags.Error},
				new LogEntry {RawContent = "E", LogLevel = LevelFlags.Fatal},
			});
			_taskScheduler.RunOnce();

			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(5);
			var entry1 = logFile.GetEntry(0);
			entry1.Index.Should().Be(0);
			entry1.LogEntryIndex.Should().Be(0);
			entry1.RawContent.Should().Be("A");
			entry1.LogLevel.Should().Be(LevelFlags.Debug);

			var entry2 = logFile.GetEntry(1);
			entry2.Index.Should().Be(1);
			entry2.LogEntryIndex.Should().Be(1);
			entry2.RawContent.Should().Be("B");
			entry2.LogLevel.Should().Be(LevelFlags.Info);
			
			var entry3 = logFile.GetEntry(2);
			entry3.Index.Should().Be(2);
			entry3.LogEntryIndex.Should().Be(2);
			entry3.RawContent.Should().Be("C");
			entry3.LogLevel.Should().Be(LevelFlags.Warning);
			
			var entry4 = logFile.GetEntry(3);
			entry4.Index.Should().Be(3);
			entry4.LogEntryIndex.Should().Be(3);
			entry4.RawContent.Should().Be("D");
			entry4.LogLevel.Should().Be(LevelFlags.Error);
			
			 var entry5 = logFile.GetEntry(4);
			entry5.Index.Should().Be(4);
			entry5.LogEntryIndex.Should().Be(4);
			entry5.RawContent.Should().Be("E");
			entry5.LogLevel.Should().Be(LevelFlags.Fatal);
		}

		[Test]
		[Description("Verifies that the log file correctly interprets many single line log entries")]
		public void TestManyEntries2()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);

			source.AddEntry("A", LevelFlags.Debug);
			_taskScheduler.RunOnce();
			
			source.AddEntry("B", LevelFlags.Info);
			_taskScheduler.RunOnce();
			
			source.AddEntry("C", LevelFlags.Warning);
			_taskScheduler.RunOnce();
			
			source.AddEntry("D", LevelFlags.Error);
			_taskScheduler.RunOnce();
			
			source.AddEntry("E", LevelFlags.Fatal);
			_taskScheduler.RunOnce();

			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(5);
			var entries = logFile.GetEntries(new LogSourceSection(0, 5));
			entries[0].Index.Should().Be(0);
			entries[0].LogEntryIndex.Should().Be(0);
			entries[0].RawContent.Should().Be("A");
			entries[0].LogLevel.Should().Be(LevelFlags.Debug);
			entries[1].Index.Should().Be(1);
			entries[1].LogEntryIndex.Should().Be(1);
			entries[1].RawContent.Should().Be("B");
			entries[1].LogLevel.Should().Be(LevelFlags.Info);
			entries[2].Index.Should().Be(2);
			entries[2].LogEntryIndex.Should().Be(2);
			entries[2].RawContent.Should().Be("C");
			entries[2].LogLevel.Should().Be(LevelFlags.Warning);
			entries[3].Index.Should().Be(3);
			entries[3].LogEntryIndex.Should().Be(3);
			entries[3].RawContent.Should().Be("D");
			entries[3].LogLevel.Should().Be(LevelFlags.Error);
			entries[4].Index.Should().Be(4);
			entries[4].LogEntryIndex.Should().Be(4);
			entries[4].RawContent.Should().Be("E");
			entries[4].LogLevel.Should().Be(LevelFlags.Fatal);
		}

		[Test]
		[Description("Verifies that the log file correctly interprets many single line log entries")]
		public void TestManyEntries3()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			for (int i = 0; i < 10001; ++i)
			{
				source.AddEntry("", LevelFlags.Info);
			}
			_taskScheduler.RunOnce();

			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(10000, "because the log file should process a fixed amount of lines per tick");
			logFile.GetProperty(Core.Properties.PercentageProcessed).Should().BeLessThan(Percentage.HundredPercent, "because the log file hasn't processed the entire source yet");

			var actualEntries = logFile.GetEntries(new LogSourceSection(0, 10000));
			for (int i = 0; i < actualEntries.Count; ++i)
			{
				actualEntries[i].Should().Be(source.GetEntry(i));
			}

			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.LogEntryCount).Should()
				.Be(10001, "because the log file should now have enough ticks elapsed to have processed the entire source");
			logFile.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent, "because the log file should've processed the entire source by now");
			actualEntries = logFile.GetEntries(new LogSourceSection(0, source.Count));
			for (int i = 0; i < actualEntries.Count; ++i)
			{
				actualEntries[i].Should().Be(source.GetEntry(i));
			}
		}

		[Test]
		public void TestManyEntries4()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);

			source.AddRange(new []
			{
				new LogEntry{RawContent = "Foo", LogLevel = LevelFlags.Other},
				new LogEntry{RawContent = "INFO: Bar", LogLevel = LevelFlags.Info},
			});
			_taskScheduler.RunOnce();

			var entries = logFile.GetEntries(new LogSourceSection(0, 2));
			entries[0].Index.Should().Be(0);
			entries[0].LogEntryIndex.Should().Be(0);
			entries[0].RawContent.Should().Be("Foo");
			entries[0].LogLevel.Should().Be(LevelFlags.Other);
			entries[1].Index.Should().Be(1);
			entries[1].LogEntryIndex.Should().Be(1);
			entries[1].RawContent.Should().Be("INFO: Bar");
			entries[1].LogLevel.Should().Be(LevelFlags.Info);

			source.RemoveFrom(1);
			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(1);

			source.AddRange(new []
			{
				new LogEntry{RawContent = "Bar", LogLevel = LevelFlags.Other},
				new LogEntry{RawContent = "INFO: Sup", LogLevel = LevelFlags.Info}
			});
			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(3);

			entries = logFile.GetEntries(new LogSourceSection(0, 3));
			entries[0].Index.Should().Be(0);
			entries[0].LogEntryIndex.Should().Be(0);
			entries[0].RawContent.Should().Be("Foo");
			entries[0].LogLevel.Should().Be(LevelFlags.Other);
			entries[1].Index.Should().Be(1);
			entries[1].LogEntryIndex.Should().Be(0);
			entries[1].RawContent.Should().Be("Bar");
			entries[1].LogLevel.Should().Be(LevelFlags.Other);
			entries[2].Index.Should().Be(2);
			entries[2].LogEntryIndex.Should().Be(1);
			entries[2].RawContent.Should().Be("INFO: Sup");
			entries[2].LogLevel.Should().Be(LevelFlags.Info);
		}

		[Test]
		[Description("Verifies that the log file correctly processes multiple events in one run")]
		public void TestManyEntries5()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			source.AddRange(new []
			{
				new LogEntry{RawContent = "Foo", LogLevel = LevelFlags.Other},
				new LogEntry{RawContent = "Bar", LogLevel = LevelFlags.Other},
				new LogEntry{RawContent = "INFO: Sup", LogLevel = LevelFlags.Info},
			});
			source.RemoveFrom(1);
			source.AddRange(new []
			{
				new LogEntry{RawContent = "Bar", LogLevel = LevelFlags.Other},
				new LogEntry{RawContent = "INFO: Sup", LogLevel = LevelFlags.Info},
			});
			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(3);

			var entries = logFile.GetEntries(new LogSourceSection(0, 3));
			entries[0].Index.Should().Be(0);
			entries[0].LogEntryIndex.Should().Be(0);
			entries[0].RawContent.Should().Be("Foo");
			entries[0].LogLevel.Should().Be(LevelFlags.Other);
			entries[1].Index.Should().Be(1);
			entries[1].LogEntryIndex.Should().Be(0);
			entries[1].RawContent.Should().Be("Bar");
			entries[1].LogLevel.Should().Be(LevelFlags.Other);
			entries[2].Index.Should().Be(2);
			entries[2].LogEntryIndex.Should().Be(1);
			entries[2].RawContent.Should().Be("INFO: Sup");
			entries[2].LogLevel.Should().Be(LevelFlags.Info);
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/74")]
		public void TestManyEntries6()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			logFile.AddListener(_listener.Object, TimeSpan.Zero, 3);

			source.AddEntry("A", LevelFlags.Other);
			_taskScheduler.RunOnce();

			source.AddEntry("B", LevelFlags.Other);
			_taskScheduler.RunOnce();
			
			source.AddEntry("C", LevelFlags.Info);
			_taskScheduler.RunOnce();

			_modifications.Should().Equal(LogSourceModification.Reset(),
				LogSourceModification.Appended(0, 1),
				LogSourceModification.Appended(1, 1),
				LogSourceModification.Appended(2, 1));
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/74")]
		public void TestManyEntries7()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, _source.Object, TimeSpan.Zero);
			logFile.AddListener(_listener.Object, TimeSpan.Zero, 3);
			
			source.AddEntry("A", LevelFlags.Other);
			_taskScheduler.RunOnce();
			
			source.AddEntry("B", LevelFlags.Other);
			_taskScheduler.RunOnce();

			source.RemoveFrom(1);
			source.AddEntry("B", LevelFlags.Other);
			source.AddEntry("C", LevelFlags.Other);
			_taskScheduler.RunOnce();
		}

		[Test]
		[Description("Verifies that GetEntries can return many entries")]
		public void TestGetEntries()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			source.AddEntry("A", LevelFlags.Debug);
			source.AddEntry("B", LevelFlags.Info);
			source.AddEntry("C", LevelFlags.Warning);
			source.AddEntry("D", LevelFlags.Error);
			source.AddEntry("E", LevelFlags.Fatal);
			_taskScheduler.RunOnce();

			var entries = logFile.GetEntries(new LogSourceSection(0, 5));
			entries[0].Index.Should().Be(0);
			entries[0].LogEntryIndex.Should().Be(0);
			entries[0].RawContent.Should().Be("A");
			entries[0].LogLevel.Should().Be(LevelFlags.Debug);
			entries[1].Index.Should().Be(1);
			entries[1].LogEntryIndex.Should().Be(1);
			entries[1].RawContent.Should().Be("B");
			entries[1].LogLevel.Should().Be(LevelFlags.Info);
			entries[2].Index.Should().Be(2);
			entries[2].LogEntryIndex.Should().Be(2);
			entries[2].RawContent.Should().Be("C");
			entries[2].LogLevel.Should().Be(LevelFlags.Warning);
			entries[3].Index.Should().Be(3);
			entries[3].LogEntryIndex.Should().Be(3);
			entries[3].RawContent.Should().Be("D");
			entries[3].LogLevel.Should().Be(LevelFlags.Error);
			entries[4].Index.Should().Be(4);
			entries[4].LogEntryIndex.Should().Be(4);
			entries[4].RawContent.Should().Be("E");
			entries[4].LogLevel.Should().Be(LevelFlags.Fatal);
		}

		[Test]
		[Description("Verifies that accessing non-existing rows is tolerated")]
		public void TestGetTimestamp1()
		{
			var source = new InMemoryLogSource();
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			var timestamps = logFile.GetColumn(new LogSourceSection(0, 1), Core.Columns.Timestamp);
			timestamps.Should().NotBeNull();
			timestamps.Should().Equal(new object[] {null}, "because accessing non-existant rows should simply return default values");
		}

		[Test]
		public void TestGetTimestamp2()
		{
			var source = new InMemoryLogSource();

			var timestamp = DateTime.UtcNow;
			source.AddEntry("", LevelFlags.Other, timestamp);

			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			_taskScheduler.RunOnce();

			var timestamps = logFile.GetColumn(new LogSourceSection(0, 1), Core.Columns.Timestamp);
			timestamps.Should().NotBeNull();
			timestamps.Should().Equal(new object[] { timestamp });
		}

		[Test]
		public void TestGetTimestamp3()
		{
			var source = new InMemoryLogSource();

			var timestamp1 = new DateTime(2017, 12, 11, 20, 33, 0);
			source.AddEntry("", LevelFlags.Debug, timestamp1);

			var timestamp2 = new DateTime(2017, 12, 11, 20, 34, 0);
			source.AddEntry("", LevelFlags.Debug, timestamp2);

			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			_taskScheduler.RunOnce();

			var timestamps = logFile.GetColumn(new LogSourceSection(0, 2), Core.Columns.Timestamp);
			timestamps.Should().NotBeNull();
			timestamps.Should().Equal(new object[] { timestamp1, timestamp2 });
		}

		[Test]
		[Ignore("Not yet implemented, maybe never will due to https://github.com/Kittyfisto/Tailviewer/issues/140")]
		[Description("Verifies that every line of a log entry provides access to the timestamp")]
		public void TestGetTimestamp4()
		{
			var source = new InMemoryLogSource();

			var timestamp1 = new DateTime(2017, 12, 11, 20, 33, 0);
			source.AddEntry("", LevelFlags.Debug, timestamp1);

			var timestamp2 = new DateTime(2017, 12, 11, 20, 34, 0);
			source.AddEntry("", LevelFlags.Debug, timestamp2);
			source.AddEntry("", LevelFlags.Other);

			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);

			var timestamps = logFile.GetColumn(new LogSourceSection(1, 2), Core.Columns.Timestamp);
			timestamps.Should().NotBeNull();
			timestamps.Should().Equal(new object[] { timestamp2, timestamp2 });
		}

		[Test]
		[Description("Verifies that every line of a log entry provides access to the timestamp")]
		public void TestGetTimestampBySection()
		{
			var source = new InMemoryLogSource();

			var timestamp1 = new DateTime(2017, 12, 11, 20, 33, 0);
			source.AddEntry("", LevelFlags.Debug, timestamp1);

			var timestamp2 = new DateTime(2017, 12, 11, 20, 34, 0);
			source.AddEntry("", LevelFlags.Debug, timestamp2);
			source.AddEntry("", LevelFlags.Other);

			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			_taskScheduler.RunOnce();

			var timestamps = logFile.GetColumn(new LogSourceSection(0, 3), Core.Columns.Timestamp);
			timestamps.Should().NotBeNull();
			timestamps.Should().Equal(new object[] { timestamp1, timestamp2, timestamp2 });
		}

		[Test]
		[Description("Verifies that every line of a log entry provides access to the timestamp")]
		public void TestGetTimestampByIndices()
		{
			var source = new InMemoryLogSource();

			var timestamp1 = new DateTime(2017, 12, 11, 20, 33, 0);
			source.AddEntry("", LevelFlags.Debug, timestamp1);

			var timestamp2 = new DateTime(2017, 12, 11, 20, 34, 0);
			source.AddEntry("", LevelFlags.Debug, timestamp2);
			source.AddEntry("", LevelFlags.Other);

			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			_taskScheduler.RunOnce();

			var timestamps = logFile.GetColumn(new LogLineIndex[] {0, 1, 2}, Core.Columns.Timestamp);
			timestamps.Should().NotBeNull();
			timestamps.Should().Equal(new object[] { timestamp1, timestamp2, timestamp2 });
		}

		[Test]
		public void TestGetEntriesBySection()
		{
			var logFile = new MultiLineLogSource(_taskScheduler, _source.Object, TimeSpan.Zero);
			var section = new LogSourceSection(42, 5);
			var buffer = new LogBufferArray(3, Core.Columns.DeltaTime, Core.Columns.RawContent);
			var destinationIndex = 2;
			var queryOptions = new LogSourceQueryOptions(LogSourceQueryMode.FromCache);

			logFile.GetEntries(section, buffer, destinationIndex, queryOptions);

			_source.Verify(x => x.GetEntries(section,
			                                 buffer,
			                                 destinationIndex,
			                                 queryOptions),
			               Times.Once);
		}

		[Test]
		public void TestGetEntriesBySection_Partial()
		{
			var source = new InMemoryLogSource();
			source.AddEntry("INFO: This is a good start", LevelFlags.Info, new DateTime(2021, 02, 11, 21, 02, 09));
			source.AddEntry("DEBUG: Hello", LevelFlags.Debug, new DateTime(2021, 02, 11, 21, 04, 09));
			source.AddEntry("\tWorld!");
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			_taskScheduler.RunOnce();

			var section = new LogSourceSection(1, 2);
			var buffer = new LogBufferArray(4, Core.Columns.Timestamp, Core.Columns.RawContent);
			var destinationIndex = 2;

			logFile.GetEntries(section, buffer, destinationIndex);
			buffer[2].RawContent.Should().Be("DEBUG: Hello");
			buffer[2].Timestamp.Should().Be(new DateTime(2021, 02, 11, 21, 04, 09));
			buffer[3].RawContent.Should().Be("\tWorld!");
			buffer[3].Timestamp.Should().Be(new DateTime(2021, 02, 11, 21, 04, 09));
		}

		[Test]
		public void TestGetEntriesByIndices()
		{
			var logFile = new MultiLineLogSource(_taskScheduler, _source.Object, TimeSpan.Zero);
			var indices = new LogLineIndex[] { 0, 2, 5 };
			var buffer = new LogBufferArray(5, Core.Columns.RawContent);
			var destinationIndex = 2;
			var queryOptions = new LogSourceQueryOptions(LogSourceQueryMode.FromCache);

			logFile.GetEntries(indices, buffer, destinationIndex, queryOptions);

			_source.Verify(x => x.GetEntries(It.Is<IReadOnlyList<LogLineIndex>>(y => y == indices),
			                                 buffer,
			                                 destinationIndex,
			                                 queryOptions),
			               Times.Once);
		}

		[Test]
		public void TestGetEntriesByIndices_Partial()
		{
			var source = new InMemoryLogSource();
			source.AddEntry("INFO: This is a good start", LevelFlags.Info, new DateTime(2021, 02, 11, 21, 02, 09));
			source.AddEntry("DEBUG: Hello", LevelFlags.Debug, new DateTime(2021, 02, 11, 21, 04, 09));
			source.AddEntry("\tWorld!");
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			_taskScheduler.RunOnce();

			var sourceIndices = new[] {new LogLineIndex(2), new LogLineIndex(1)};
			var buffer = new LogBufferArray(4, Core.Columns.Timestamp, Core.Columns.RawContent);
			var destinationIndex = 1;

			logFile.GetEntries(sourceIndices, buffer, destinationIndex);
			buffer[1].RawContent.Should().Be("\tWorld!");
			buffer[1].Timestamp.Should().Be(new DateTime(2021, 02, 11, 21, 04, 09));
			buffer[2].RawContent.Should().Be("DEBUG: Hello");
			buffer[2].Timestamp.Should().Be(new DateTime(2021, 02, 11, 21, 04, 09));
		}

		[Test]
		public void TestMultipleLinesOneEntry()
		{
			var source = new InMemoryLogSource();
			source.AddEntry("2017-12-03 11:59:30 Hello, ", LevelFlags.Other, new DateTime(2017, 12, 3, 11, 59, 30));
			source.AddEntry("World!", LevelFlags.Other);
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);

			_taskScheduler.RunOnce();

			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(2);

			var entries = logFile.GetEntries(new[] { new LogLineIndex(0), new LogLineIndex(1) },
				new IColumnDescriptor[]
				{
					Core.Columns.LineNumber, Core.Columns.LogEntryIndex, Core.Columns.Timestamp,
					Core.Columns.RawContent
				});
			var line = entries[0];
			line.GetValue(Core.Columns.LineNumber).Should().Be(1);
			line.GetValue(Core.Columns.LogEntryIndex).Should().Be(0);
			line.RawContent.Should().Be("2017-12-03 11:59:30 Hello, ");
			line.Timestamp.Should().Be(new DateTime(2017, 12, 3, 11, 59, 30));

			line = entries[1];
			line.GetValue(Core.Columns.LineNumber).Should().Be(2);
			line.GetValue(Core.Columns.LogEntryIndex).Should().Be(0);
			line.RawContent.Should().Be("World!");
			line.Timestamp.Should().Be(new DateTime(2017, 12, 3, 11, 59, 30));
		}

		[Test]
		public void TestMultipleSingleEntries()
		{
			var source = new InMemoryLogSource();
			source.AddEntry("2019-07-08 16:19:13.546 [TID = 01428] [CSomeClass::FooBar()] [Session 1337] [FW]", LevelFlags.Other, new DateTime(2019, 7, 8, 16, 19, 13, 546));
			source.AddEntry("2019-07-08 16:19:13.546 [TID = 01428] [CSomeClass::FooBar()] [Session 1337] [FW] [IND_ERROR] [The test case had to be terminated.] [The stack has indicated an UNKNOWN error.SAP Name = 'EM', AppName = 'lte-l1c-phy.elf', StatusInfo = '<missing>']", LevelFlags.Other, new DateTime(2019, 7, 8, 16, 19, 13, 546));
			source.AddEntry("2019-07-08 16:19:13.546 [TID = 01428] [CSomeOtherClass::Ooopsie()][Whoops] [to] [eTC_STOP_EVENT_SUSPEND_IND]", LevelFlags.Other, new DateTime(2019, 7, 8, 16, 19, 13, 546));

			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			_taskScheduler.RunOnce();
			logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(3);
			var entries = logFile.GetEntries(new LogSourceSection(0, 3), new[]{Core.Columns.LogEntryIndex});
			entries[0].LogEntryIndex.Should().Be(new LogEntryIndex(0));
			entries[1].LogEntryIndex.Should().Be(new LogEntryIndex(1));
			entries[2].LogEntryIndex.Should().Be(new LogEntryIndex(2));
		}

		[Test]
		[Description("Verifies that if a log file consistently has timestamps to distinguish log entries, the presence of log levels (or lack thereof) does not cause a misclassification of log entries")]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/221")]
		public void TestTimestampsAndMixedInfos()
		{
			var logFile = new InMemoryLogSource();

			using (var multiLine = new MultiLineLogSource(_taskScheduler, logFile, TimeSpan.Zero))
			{
				logFile.AddRange(new[]
				{
					new LogEntry{Timestamp = new DateTime(2017, 3, 24, 11, 45, 19, 195), LogLevel = LevelFlags.Info, RawContent = "2017-03-24 11-45-19.195339; 0; 0;  0; 108;  0; 124;   1;INFO; ; ; ; ; ; 0; Some interesting message"},
					new LogEntry{Timestamp = new DateTime(2017, 3, 24, 11, 45, 19, 751), LogLevel = LevelFlags.Info, RawContent = "2017-03-24 11-45-19.751428; 0; 0;  0; 129;  0; 145;   1;INFO; ; ; ; ; ; 0; Very interesting stuff"},
					new LogEntry{Timestamp = new DateTime(2017, 3, 24, 11, 45, 21, 708), LogLevel = LevelFlags.Other, RawContent = "2017-03-24 11-45-21.708485; 0; 0;  0; 109;  0; 125;   1;PB_CREATE; ; ; 109; 2;"}
				});

				_taskScheduler.RunOnce();
				multiLine.GetProperty(Core.Properties.LogEntryCount).Should().Be(3);

				var line0 = multiLine.GetEntry(0);
				line0.OriginalIndex.Should().Be(0);
				line0.Index.Should().Be(0);
				line0.LogEntryIndex.Should().Be(0, "because every line is an individual log entry");
				line0.Timestamp.Should().Be(new DateTime(2017, 3, 24, 11, 45, 19, 195));

				var line1 = multiLine.GetEntry(1);
				line1.OriginalIndex.Should().Be(1);
				line1.Index.Should().Be(1);
				line1.LogEntryIndex.Should().Be(1, "because every line is an individual log entry");
				line1.Timestamp.Should().Be(new DateTime(2017, 3, 24, 11, 45, 19, 751));

				var line2 = multiLine.GetEntry(2);
				line2.OriginalIndex.Should().Be(2);
				line2.Index.Should().Be(2);
				line2.LogEntryIndex.Should().Be(2, "because every line is an individual log entry");
				line2.Timestamp.Should().Be(new DateTime(2017, 3, 24, 11, 45, 21, 708));


				logFile.RemoveFrom(new LogLineIndex(2));
				logFile.AddRange(new []
				{
					new LogEntry{Timestamp = new DateTime(2017, 3, 24, 11, 45, 21, 708), LogLevel = LevelFlags.Other, RawContent = "2017-03-24 11-45-21.708485; 0; 0;  0; 109;  0; 125;   1;PB_CREATE; ; ; 109; 2; Sooo interesting"},
					new LogEntry{Timestamp = new DateTime(2017, 3, 24, 11, 45, 21, 708), LogLevel = LevelFlags.Info, RawContent = "2017-03-24 11-45-21.708599; 0; 0;  0; 108;  0; 124;   1;INFO; ; ; ; ; ; 0; Go on!"},
					new LogEntry{Timestamp = new DateTime(2017, 3, 24, 11, 45, 21, 811), LogLevel = LevelFlags.Info, RawContent = "2017-03-24 11-45-21.811838; 0; 0;  0; 108;  0; 124;   1;INFO; ; ; ; ; ; 0; done."}
				});
				_taskScheduler.RunOnce();
				multiLine.GetProperty(Core.Properties.LogEntryCount).Should().Be(5);

				line0 = multiLine.GetEntry(0);
				line0.OriginalIndex.Should().Be(0);
				line0.Index.Should().Be(0);
				line0.LogEntryIndex.Should().Be(0, "because every line is an individual log entry");
				line0.Timestamp.Should().Be(new DateTime(2017, 3, 24, 11, 45, 19, 195));

				line1 = multiLine.GetEntry(1);
				line1.OriginalIndex.Should().Be(1);
				line1.Index.Should().Be(1);
				line1.LogEntryIndex.Should().Be(1, "because every line is an individual log entry");
				line1.Timestamp.Should().Be(new DateTime(2017, 3, 24, 11, 45, 19, 751));

				line2 = multiLine.GetEntry(2);
				line2.OriginalIndex.Should().Be(2);
				line2.Index.Should().Be(2);
				line2.LogEntryIndex.Should().Be(2, "because every line is an individual log entry");
				line2.Timestamp.Should().Be(new DateTime(2017, 3, 24, 11, 45, 21, 708));

				var line3 = multiLine.GetEntry(3);
				line3.OriginalIndex.Should().Be(3);
				line3.Index.Should().Be(3);
				line3.LogEntryIndex.Should().Be(3, "because every line is an individual log entry");
				line3.Timestamp.Should().Be(new DateTime(2017, 3, 24, 11, 45, 21, 708));

				var line4 = multiLine.GetEntry(4);
				line4.OriginalIndex.Should().Be(4);
				line4.Index.Should().Be(4);
				line4.LogEntryIndex.Should().Be(4, "because every line is an individual log entry");
				line4.Timestamp.Should().Be(new DateTime(2017, 3, 24, 11, 45, 21, 811));
			}
		}

		protected override ILogSource CreateEmpty()
		{
			var source = new InMemoryLogSource();
			return new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
		}

		protected override ILogSource CreateFromContent(IReadOnlyLogBuffer content)
		{
			var source = new InMemoryLogSource(content);
			var logFile = new MultiLineLogSource(_taskScheduler, source, TimeSpan.Zero);
			_taskScheduler.RunOnce();
			return logFile;
		}
	}
}
