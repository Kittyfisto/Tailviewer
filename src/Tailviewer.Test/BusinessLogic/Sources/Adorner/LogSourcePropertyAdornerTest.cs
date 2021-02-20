﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Core;
using Tailviewer.Core.Buffers;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Entries;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources;
using Tailviewer.Core.Sources.Adorner;

namespace Tailviewer.Test.BusinessLogic.Sources.Adorner
{
	[TestFixture]
	public sealed class LogSourcePropertyAdornerTest
		: AbstractAggregatedLogSourceTest
	{
		private Mock<ILogSource> _source;
		private LogSourceListenerCollection _listeners;
		private ManualTaskScheduler _scheduler;
		private LogBufferList _entries;
		private PropertiesBufferList _sourceProperties;

		[SetUp]
		public void Setup2()
		{
			_scheduler = new ManualTaskScheduler();
			_source = new Mock<ILogSource>();
			_listeners = new LogSourceListenerCollection(_source.Object);
			_entries = new LogBufferList(GeneralColumns.Index, GeneralColumns.Timestamp);
			_sourceProperties = new PropertiesBufferList();
			_source.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
			       .Callback((IPropertiesBuffer destination) => _sourceProperties.CopyAllValuesTo(destination));
			_source.Setup(x => x.GetProperty(It.IsAny<IPropertyDescriptor>()))
			       .Returns((IPropertyDescriptor property) => _sourceProperties.GetValue(property));

			_source.Setup(x => x.Columns).Returns(() => _entries.Columns);
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
				       _entries.CopyTo(new Int32View(sourceIndices), destination, destinationIndex);
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
			_source.Setup(x => x.GetProperty(GeneralProperties.Format)).Returns(format.Object);
			_source.Setup(x => x.GetProperty((IReadOnlyPropertyDescriptor)GeneralProperties.Format)).Returns((object)format.Object);

			adorner.GetProperty(GeneralProperties.Format).Should().Be(format.Object,
			                                                          "because the adorner should forward GetProperty calls to the source when they aren't adorned and return its return value");
			_source.Verify(x => x.GetProperty(GeneralProperties.Format), Times.Once);


			adorner.GetProperty((IReadOnlyPropertyDescriptor)GeneralProperties.Format).Should().Be(format.Object,
			                                                          "because the adorner should forward GetProperty calls to the source when they aren't adorned and return its return value");
			_source.Verify(x => x.GetProperty((IReadOnlyPropertyDescriptor)GeneralProperties.Format), Times.Once);
		}

		[Test]
		public void TestGetPartiallyAdornedProperties()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			_entries.Add(new LogEntry
			{
				Index = 0,
				Timestamp = new DateTime(2021, 02, 20, 18, 31, 45)
			});
			_listeners.OnRead(_entries.Count);
			_scheduler.RunOnce();

			_source.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>())).Callback((IPropertiesBuffer
				destination) =>
			{
				destination.SetValue(TextProperties.Encoding, Encoding.UTF32);
			});

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

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().BeNull();
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().BeNull();
		}

		[Test]
		public void TestOneEntryNoTimestampColumn()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);
			_scheduler.RunOnce();
			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().BeNull();
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().BeNull();

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().BeNull();
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().BeNull();
		}

		[Test]
		public void TestOneEntryNullTimestamp()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);
			_scheduler.RunOnce();
			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().BeNull();
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().BeNull();

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().BeNull();
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().BeNull();
		}

		[Test]
		public void TestOneEntryTimestamp()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			_entries.Add(new LogEntry
			{
				Index = 0,
				Timestamp = new DateTime(2021, 02, 20, 17, 52, 31)
			});
			_listeners.OnRead(_entries.Count);

			_scheduler.RunOnce();
			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));
		}

		[Test]
		public void TestTwoEntriesAscendingTimestamp()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			_entries.Add(new LogEntry
			{
				Index = 0,
				Timestamp = new DateTime(2021, 02, 20, 17, 52, 31)
			});
			_listeners.OnRead(_entries.Count);
			_scheduler.RunOnce();

			_entries.Add(new LogEntry
			{
				Index = 1,
				Timestamp = new DateTime(2021, 02, 20, 18, 02, 12)
			});
			_listeners.OnRead(_entries.Count);
			_scheduler.RunOnce();

			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2021, 02, 20, 18, 02, 12));

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2021, 02, 20, 18, 02, 12));
		}

		[Test]
		[Description("Verifies that the adorner tolerates its source being shit and not ordering its entries")]
		public void TestTwoEntriesDescendingTimestamp()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			_entries.Add(new LogEntry
			{
				Index = 0,
				Timestamp = new DateTime(2021, 02, 20, 18, 03, 37)
			});
			_listeners.OnRead(_entries.Count);
			_scheduler.RunOnce();

			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2021, 02, 20, 18, 03, 37));
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2021, 02, 20, 18, 03, 37));

			_entries.Add(new LogEntry
			{
				Index = 1,
				Timestamp = new DateTime(2021, 02, 20, 18, 02, 12)
			});
			_listeners.OnRead(_entries.Count);
			_scheduler.RunOnce();

			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2021, 02, 20, 18, 02, 12));
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2021, 02, 20, 18, 03, 37));

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2021, 02, 20, 18, 02, 12));
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2021, 02, 20, 18, 03, 37));
		}

		[Test]
		public void TestOneEntryTimestampReset()
		{
			var adorner = new LogSourcePropertyAdorner(_scheduler, _source.Object, TimeSpan.Zero);

			_entries.Add(new LogEntry
			{
				Index = 0,
				Timestamp = new DateTime(2021, 02, 20, 17, 52, 31)
			});
			_listeners.OnRead(_entries.Count);
			_scheduler.RunOnce();

			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EndTimestamp);
			adorner.GetAllProperties(buffer);
			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2021, 02, 20, 17, 52, 31));

			_entries.Clear();
			_listeners.Reset();
			_scheduler.RunOnce();
			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().BeNull();
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().BeNull();

			adorner.GetAllProperties(buffer);
			adorner.GetProperty(GeneralProperties.StartTimestamp).Should().BeNull();
			adorner.GetProperty(GeneralProperties.EndTimestamp).Should().BeNull();
		}
	}
}
