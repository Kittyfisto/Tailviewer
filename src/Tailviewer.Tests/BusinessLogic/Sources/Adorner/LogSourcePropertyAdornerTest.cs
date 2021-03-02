using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Sources;
using Tailviewer.Core.Sources.Adorner;

namespace Tailviewer.Tests.BusinessLogic.Sources.Adorner
{
	[TestFixture]
	public sealed class LogSourcePropertyAdornerTest
		: AbstractAggregatedLogSourceTest
	{
		private Mock<ILogSource> _source;
		private LogSourceListenerCollection _listeners;
		private ManualTaskScheduler _scheduler;
		private LogBufferList _sourceEntries;
		private PropertiesBufferList _sourceProperties;

		[SetUp]
		public void Setup2()
		{
			_scheduler = new ManualTaskScheduler();
			_source = new Mock<ILogSource>();
			_listeners = new LogSourceListenerCollection(_source.Object);
			_sourceEntries = new LogBufferList(GeneralColumns.Index, GeneralColumns.LogLevel, GeneralColumns.Timestamp);
			_sourceProperties = new PropertiesBufferList();
			_source.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
			       .Callback((IPropertiesBuffer destination) => _sourceProperties.CopyAllValuesTo(destination));
			_source.Setup(x => x.GetProperty(It.IsAny<IPropertyDescriptor>()))
			       .Returns((IPropertyDescriptor property) => _sourceProperties.GetValue(property));

			_source.Setup(x => x.Columns).Returns(() => _sourceEntries.Columns);
			_source.Setup(x => x.AddListener(It.IsAny<ILogSourceListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
			       .Callback((ILogSourceListener listener, TimeSpan maximumWaitTime, int maximumLineCount) =>
			       {
				       _listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
			       });
			_source.Setup(x => x.RemoveListener(It.IsAny<ILogSourceListener>()))
			       .Callback((ILogSourceListener listener) =>
			       {
				       _listeners.RemoveListener(listener);
			       });
			_source.Setup(x => x.GetEntries(It.IsAny<IReadOnlyList<LogLineIndex>>(), It.IsAny<ILogBuffer>(),
			                                It.IsAny<int>(), It.IsAny<LogSourceQueryOptions>()))
			       .Callback((IReadOnlyList<LogLineIndex> sourceIndices, ILogBuffer destination, int destinationIndex, LogSourceQueryOptions queryOptions) =>
			       {
				       _sourceEntries.CopyTo(new Int32View(sourceIndices), destination, destinationIndex);
			       });
		}

		protected override ILogSource Create(ITaskScheduler taskScheduler, ILogSource source)
		{
			return new LogSourcePropertyAdorner(taskScheduler, source, TimeSpan.Zero);
		}

		[Test]
		public void TestConstruction()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			adorner.Properties.Should().Contain(LogSourcePropertyAdorner.AllAdornedProperties);

			adorner.GetProperty(GeneralProperties.PercentageProcessed).Should().Be(Percentage.Zero, "because no processing has been done just yet");

			var buffer = new PropertiesBufferList();
			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.PercentageProcessed).Should().Be(Percentage.Zero, "because no processing has been done just yet");
		}

		[Test]
		public void TestEmptySourceFinishedProcessing()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			_sourceProperties.SetValue(GeneralProperties.PercentageProcessed, Percentage.HundredPercent);
			_scheduler.RunOnce();

			adorner.GetProperty(GeneralProperties.PercentageProcessed).Should().Be(Percentage.HundredPercent, "because both the source and adorner are finished processing");

			var buffer = new PropertiesBufferList();
			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.PercentageProcessed).Should().Be(Percentage.HundredPercent, "because both the source and adorner are finished processing");
		}

		[Test]
		[Description("Verifies that all set property calls are forwarded to the source")]
		public void TestSetProperty()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);
			adorner.SetProperty(TextProperties.OverwrittenEncoding, Encoding.BigEndianUnicode);
			_source.Verify(x => x.SetProperty(TextProperties.OverwrittenEncoding, Encoding.BigEndianUnicode), Times.Once);
			
			adorner.SetProperty((IPropertyDescriptor)TextProperties.OverwrittenEncoding, Encoding.BigEndianUnicode);
			_source.Verify(x => x.SetProperty((IPropertyDescriptor)TextProperties.OverwrittenEncoding, Encoding.BigEndianUnicode), Times.Once);
		}

		[Test]
		[Description("Verifies that all get property calls for properties which are not adorned are forwarded to the source")]
		public void TestGetNonAdornedProperty()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			var format = new Mock<ILogFileFormat>();
			_sourceProperties.SetValue(GeneralProperties.Format, format.Object);
			_scheduler.RunOnce();

			adorner.GetProperty(GeneralProperties.Format).Should().Be(format.Object,
			                                                          "because the adorner should forward GetProperty calls to the source when they aren't adorned and return its return value");
			adorner.GetProperty((IReadOnlyPropertyDescriptor)GeneralProperties.Format).Should().Be(format.Object,
			                                                          "because the adorner should forward GetProperty calls to the source when they aren't adorned and return its return value");
		}

		[Test]
		public void TestGetPartiallyAdornedProperties()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			_sourceEntries.Add(new LogEntry
			{
				Index = 0,
				Timestamp = new DateTime(2021, 02, 20, 18, 31, 45)
			});
			_listeners.OnRead(_sourceEntries.Count);
			_sourceProperties.SetValue(TextProperties.Encoding, Encoding.UTF32);
			_scheduler.RunOnce();

			var buffer = new PropertiesBufferList();
			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2021, 02, 20, 18, 31, 45));
			buffer.GetValue(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2021, 02, 20, 18, 31, 45));
			buffer.GetValue(TextProperties.Encoding).Should().Be(Encoding.UTF32, "because those properties which are not adorned should have been retrieved from the source");
		}

		[Test]
		public void TestEmpty()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);
			_scheduler.RunOnce();
			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().BeNull();
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().BeNull();
			adorner.GetProperty(GeneralProperties.Duration).Should().BeNull();

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.StartTimestamp).Should().BeNull();
			buffer.GetValue(GeneralProperties.EndTimestamp).Should().BeNull();
			buffer.GetValue(GeneralProperties.Duration).Should().BeNull();
		}

		[Test]
		public void TestOneEntryNoTimestampColumn()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);
			_scheduler.RunOnce();
			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().BeNull();
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().BeNull();
			adorner.GetProperty(GeneralProperties.Duration).Should().BeNull();

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.StartTimestamp).Should().BeNull();
			buffer.GetValue(GeneralProperties.EndTimestamp).Should().BeNull();
			buffer.GetValue(GeneralProperties.Duration).Should().BeNull();
		}

		[Test]
		public void TestOneEntryNullTimestamp()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			_sourceEntries.Add(new LogEntry
			{
				Index = 0,
				Timestamp = null
			});

			_scheduler.RunOnce();
			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().BeNull();
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().BeNull();
			adorner.GetProperty(GeneralProperties.Duration).Should().BeNull();

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.StartTimestamp).Should().BeNull();
			buffer.GetValue(GeneralProperties.EndTimestamp).Should().BeNull();
			buffer.GetValue(GeneralProperties.Duration).Should().BeNull();
		}

		[Test]
		public void TestOneEntryTimestamp()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			_sourceEntries.Add(new LogEntry
			{
				Index = 0,
				Timestamp = new DateTime(2021, 02, 20, 17, 52, 31)
			});
			_listeners.OnRead(_sourceEntries.Count);

			_scheduler.RunOnce();
			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));
			adorner.GetProperty(GeneralProperties.Duration).Should().Be(TimeSpan.Zero);

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));
			buffer.GetValue(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));
			buffer.GetValue(GeneralProperties.Duration).Should().Be(TimeSpan.Zero);
		}

		[Test]
		public void TestTwoEntriesAscendingTimestamp()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			_sourceEntries.Add(new LogEntry
			{
				Index = 0,
				Timestamp = new DateTime(2021, 02, 20, 17, 52, 31)
			});
			_listeners.OnRead(_sourceEntries.Count);
			_scheduler.RunOnce();

			_sourceEntries.Add(new LogEntry
			{
				Index = 1,
				Timestamp = new DateTime(2021, 02, 20, 18, 02, 12)
			});
			_listeners.OnRead(_sourceEntries.Count);
			_scheduler.RunOnce();

			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2021, 02, 20, 18, 02, 12));
			adorner.GetProperty(GeneralProperties.Duration).Should().Be(TimeSpan.FromSeconds(581));

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));
			buffer.GetValue(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2021, 02, 20, 18, 02, 12));
			adorner.GetProperty(GeneralProperties.Duration).Should().Be(TimeSpan.FromSeconds(581));
		}

		[Test]
		[Description("Verifies that the adorner tolerates its source being shit and not ordering its entries")]
		public void TestTwoEntriesDescendingTimestamp()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			_sourceEntries.Add(new LogEntry
			{
				Index = 0,
				Timestamp = new DateTime(2021, 02, 20, 18, 03, 37)
			});
			_listeners.OnRead(_sourceEntries.Count);
			_scheduler.RunOnce();

			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2021, 02, 20, 18, 03, 37));
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2021, 02, 20, 18, 03, 37));
			adorner.GetProperty(GeneralProperties.Duration).Should().Be(TimeSpan.Zero);

			_sourceEntries.Add(new LogEntry
			{
				Index = 1,
				Timestamp = new DateTime(2021, 02, 20, 18, 02, 12)
			});
			_listeners.OnRead(_sourceEntries.Count);
			_scheduler.RunOnce();

			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2021, 02, 20, 18, 02, 12));
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2021, 02, 20, 18, 03, 37));
			adorner.GetProperty(GeneralProperties.Duration).Should().Be(TimeSpan.FromSeconds(85));

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2021, 02, 20, 18, 02, 12));
			buffer.GetValue(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2021, 02, 20, 18, 03, 37));
			buffer.GetValue(GeneralProperties.Duration).Should().Be(TimeSpan.FromSeconds(85));
		}

		[Test]
		public void TestOneEntryTimestampReset()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			_sourceEntries.Add(new LogEntry
			{
				Index = 0,
				Timestamp = new DateTime(2021, 02, 20, 17, 52, 31)
			});
			_listeners.OnRead(_sourceEntries.Count);
			_scheduler.RunOnce();

			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));
			adorner.GetProperty(GeneralProperties.Duration).Should().Be(TimeSpan.Zero);

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));
			buffer.GetValue(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));
			buffer.GetValue(GeneralProperties.Duration).Should().Be(TimeSpan.Zero);

			_sourceEntries.Clear();
			_listeners.Reset();
			_scheduler.RunOnce();
			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().BeNull();
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().BeNull();
			adorner.GetProperty(GeneralProperties.Duration).Should().BeNull();

			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.StartTimestamp).Should().BeNull();
			buffer.GetValue(GeneralProperties.EndTimestamp).Should().BeNull();
			buffer.GetValue(GeneralProperties.Duration).Should().BeNull();
		}

		[Test]
		public void TestLogLevelOther()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			_sourceEntries.Add(new LogEntry
			{
				Index = 0,
				LogLevel = LevelFlags.Other
			});
			_listeners.OnRead(_sourceEntries.Count);
			_scheduler.RunOnce();

			adorner.GetProperty(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.OtherLogEntryCount).Should().Be(1);

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.OtherLogEntryCount).Should().Be(1);

			_sourceEntries.Clear();
			_listeners.Reset();
			_scheduler.RunOnce();
			adorner.GetProperty(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.OtherLogEntryCount).Should().Be(0);

			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.OtherLogEntryCount).Should().Be(0);
		}

		[Test]
		public void TestLogLevelTrace()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			_sourceEntries.Add(new LogEntry
			{
				Index = 0,
				LogLevel = LevelFlags.Trace
			});
			_listeners.OnRead(_sourceEntries.Count);
			_scheduler.RunOnce();

			adorner.GetProperty(GeneralProperties.TraceLogEntryCount).Should().Be(1);
			adorner.GetProperty(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.OtherLogEntryCount).Should().Be(0);

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.TraceLogEntryCount).Should().Be(1);
			buffer.GetValue(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.OtherLogEntryCount).Should().Be(0);

			_sourceEntries.Clear();
			_listeners.Reset();
			_scheduler.RunOnce();
			adorner.GetProperty(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.OtherLogEntryCount).Should().Be(0);

			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.OtherLogEntryCount).Should().Be(0);
		}

		[Test]
		public void TestLogLevelDebug()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			_sourceEntries.Add(new LogEntry
			{
				Index = 0,
				LogLevel = LevelFlags.Debug
			});
			_listeners.OnRead(_sourceEntries.Count);
			_scheduler.RunOnce();

			adorner.GetProperty(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.DebugLogEntryCount).Should().Be(1);
			adorner.GetProperty(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.OtherLogEntryCount).Should().Be(0);

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.DebugLogEntryCount).Should().Be(1);
			buffer.GetValue(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.OtherLogEntryCount).Should().Be(0);

			_sourceEntries.Clear();
			_listeners.Reset();
			_scheduler.RunOnce();
			adorner.GetProperty(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.OtherLogEntryCount).Should().Be(0);

			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.OtherLogEntryCount).Should().Be(0);
		}

		[Test]
		public void TestLogLevelInfo()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			_sourceEntries.Add(new LogEntry
			{
				Index = 0,
				LogLevel = LevelFlags.Info
			});
			_listeners.OnRead(_sourceEntries.Count);
			_scheduler.RunOnce();

			adorner.GetProperty(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.InfoLogEntryCount).Should().Be(1);
			adorner.GetProperty(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.OtherLogEntryCount).Should().Be(0);

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.InfoLogEntryCount).Should().Be(1);
			buffer.GetValue(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.OtherLogEntryCount).Should().Be(0);

			_sourceEntries.Clear();
			_listeners.Reset();
			_scheduler.RunOnce();
			adorner.GetProperty(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.OtherLogEntryCount).Should().Be(0);

			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.OtherLogEntryCount).Should().Be(0);
		}

		[Test]
		public void TestLogLevelWarning()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			_sourceEntries.Add(new LogEntry
			{
				Index = 0,
				LogLevel = LevelFlags.Warning
			});
			_listeners.OnRead(_sourceEntries.Count);
			_scheduler.RunOnce();

			adorner.GetProperty(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.WarningLogEntryCount).Should().Be(1);
			adorner.GetProperty(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.OtherLogEntryCount).Should().Be(0);

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.WarningLogEntryCount).Should().Be(1);
			buffer.GetValue(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.OtherLogEntryCount).Should().Be(0);

			_sourceEntries.Clear();
			_listeners.Reset();
			_scheduler.RunOnce();
			adorner.GetProperty(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.OtherLogEntryCount).Should().Be(0);

			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.OtherLogEntryCount).Should().Be(0);
		}

		[Test]
		public void TestLogLevelError()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			_sourceEntries.Add(new LogEntry
			{
				Index = 0,
				LogLevel = LevelFlags.Error
			});
			_listeners.OnRead(_sourceEntries.Count);
			_scheduler.RunOnce();

			adorner.GetProperty(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.ErrorLogEntryCount).Should().Be(1);
			adorner.GetProperty(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.OtherLogEntryCount).Should().Be(0);

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.ErrorLogEntryCount).Should().Be(1);
			buffer.GetValue(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.OtherLogEntryCount).Should().Be(0);

			_sourceEntries.Clear();
			_listeners.Reset();
			_scheduler.RunOnce();
			adorner.GetProperty(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.OtherLogEntryCount).Should().Be(0);

			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.OtherLogEntryCount).Should().Be(0);
		}

		[Test]
		public void TestLogLevelFatal()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			_sourceEntries.Add(new LogEntry
			{
				Index = 0,
				LogLevel = LevelFlags.Fatal
			});
			_listeners.OnRead(_sourceEntries.Count);
			_scheduler.RunOnce();

			adorner.GetProperty(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.FatalLogEntryCount).Should().Be(1);
			adorner.GetProperty(GeneralProperties.OtherLogEntryCount).Should().Be(0);

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.FatalLogEntryCount).Should().Be(1);
			buffer.GetValue(GeneralProperties.OtherLogEntryCount).Should().Be(0);

			_sourceEntries.Clear();
			_listeners.Reset();
			_scheduler.RunOnce();
			adorner.GetProperty(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			adorner.GetProperty(GeneralProperties.OtherLogEntryCount).Should().Be(0);

			adorner.GetAllProperties(buffer);
			buffer.GetValue(GeneralProperties.TraceLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.DebugLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.InfoLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.WarningLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			buffer.GetValue(GeneralProperties.OtherLogEntryCount).Should().Be(0);
		}
	}
}
