using System;
using System.Collections.Generic;
using FluentAssertions;
using Metrolib;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public abstract class AbstractLogFilePropertiesTest
	{
		protected abstract ILogFileProperties Create(params KeyValuePair<IReadOnlyPropertyDescriptor, object>[] properties);

		[Test]
		public void TestDebuggerVisualization1()
		{
			var properties = Create();
			var view = new LogFilePropertiesDebuggerView(properties);
			view.Items.Should().BeEmpty();
		}

		[Test]
		public void TestDebuggerVisualization2()
		{
			var properties = Create(
			                        new KeyValuePair<IReadOnlyPropertyDescriptor, object>(LogFileProperties.Created, new DateTime(2017, 12, 29, 12, 50, 0)),
			                        new KeyValuePair<IReadOnlyPropertyDescriptor, object>(LogFileProperties.Size, Size.FromGigabytes(1))
			                        );
			var view = new LogFilePropertiesDebuggerView(properties);
			var items = view.Items;
			view.Items.Should().HaveCount(2);
			items.Should().ContainKey(LogFileProperties.Created);
			items[LogFileProperties.Created].Should().Be(new DateTime(2017, 12, 29, 12, 50, 0));
			items.Should().ContainKey(LogFileProperties.Size);
			items[LogFileProperties.Size].Should().Be(Size.FromGigabytes(1));
		}

		[Test]
		public void TestTryGetValue1()
		{
			var properties = Create();
			object unused;
			new Action(() => properties.TryGetValue(null, out unused))
				.Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestTryGetValue2()
		{
			var properties = Create();
			string unused;
			new Action(() => properties.TryGetValue(null, out unused))
				.Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestTryGetValue3()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(LogFileProperties.EmptyReason, ErrorFlags.SourceCannotBeAccessed));
			DateTime? lastModified;
			properties.TryGetValue(LogFileProperties.LastModified, out lastModified).Should().BeFalse();
			lastModified.Should().Be(LogFileProperties.LastModified.DefaultValue);
		}

		[Test]
		public void TestGetValue1()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(LogFileProperties.Created, new DateTime(2017, 12, 29, 13, 0, 0)));
			properties.GetValue(LogFileProperties.Created).Should().Be(new DateTime(2017, 12, 29, 13, 0, 0));
		}

		[Test]
		public void TestSetValue1()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(LogFileProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)));
			properties.SetValue(LogFileProperties.Created, new DateTime(2017, 12, 29, 13, 2, 0));
			properties.GetValue(LogFileProperties.Created).Should().Be(new DateTime(2017, 12, 29, 13, 2, 0));
		}

		[Test]
		[Description("Verifies that the non-generic overload works as well")]
		public void TestSetValue2()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(LogFileProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)));
			properties.SetValue((IReadOnlyPropertyDescriptor)LogFileProperties.Created, new DateTime(2017, 12, 29, 13, 2, 0));
			properties.GetValue(LogFileProperties.Created).Should().Be(new DateTime(2017, 12, 29, 13, 2, 0));
		}

		[Test]
		public void TestSetValue3()
		{
			var properties = Create();
			new Action(() => properties.SetValue(null, new DateTime(2017, 12, 29, 13, 2, 0)))
				.Should().Throw<ArgumentNullException>();
		}

		[Test]
		[Description("Verifies that the non-generic overload throws")]
		public void TestSetValue4()
		{
			var properties = Create();
			new Action(() => properties.SetValue((IReadOnlyPropertyDescriptor) null, new DateTime(2017, 12, 29, 13, 2, 0)))
				.Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestCopyAllValuesTo1()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(LogFileProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)),
			                        new KeyValuePair<IReadOnlyPropertyDescriptor, object>(LogFileProperties.StartTimestamp, new DateTime(2017, 12, 29, 13, 3, 0)));
			new Action(() => properties.CopyAllValuesTo(null)).Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestCopyAllValuesTo2()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(LogFileProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)),
			                        new KeyValuePair<IReadOnlyPropertyDescriptor, object>(LogFileProperties.StartTimestamp, new DateTime(2017, 12, 29, 13, 3, 0)));

			var buffer = new LogFilePropertyList();
			properties.CopyAllValuesTo(buffer);
			buffer.Properties.Should().Equal(properties.Properties);
			buffer.GetValue(LogFileProperties.Created).Should().Be(new DateTime(2017, 12, 29, 13, 1, 0));
			buffer.GetValue(LogFileProperties.StartTimestamp).Should().Be(new DateTime(2017, 12, 29, 13, 3, 0));
		}

		[Test]
		[Description("Verifies that it's possible to retrieve a subset of values from a properties object")]
		public void TestCopyAllValuesTo3()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(LogFileProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)),
			                    new KeyValuePair<IReadOnlyPropertyDescriptor, object>(LogFileProperties.StartTimestamp, new DateTime(2017, 12, 29, 13, 3, 0)));

			var buffer = new LogFilePropertyList();
			properties.Except(LogFileProperties.Minimum).CopyAllValuesTo(buffer);
			buffer.Properties.Should().BeEmpty();
		}

		[Test]
		[Description("Verifies that it's possible to retrieve a subset of values from a properties object")]
		public void TestCopyAllValuesTo4()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(LogFileProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)),
			                        new KeyValuePair<IReadOnlyPropertyDescriptor, object>(LogFileProperties.StartTimestamp, new DateTime(2017, 12, 29, 13, 3, 0)));

			var buffer = new LogFilePropertyList(LogFileProperties.StartTimestamp);
			properties.CopyAllValuesTo(buffer);
			buffer.GetValue(LogFileProperties.StartTimestamp).Should().Be(new DateTime(2017, 12, 29, 13, 3, 0));
		}

		[Test]
		[Description("Verifies that accessing non existing properties is allowed and returns their default value")]
		public void TestCopyAllValuesTo5()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(LogFileProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)),
			                        new KeyValuePair<IReadOnlyPropertyDescriptor, object>(LogFileProperties.StartTimestamp, new DateTime(2017, 12, 29, 13, 3, 0)));

			var buffer = new LogFilePropertyList(LogFileProperties.StartTimestamp, LogFileProperties.EmptyReason);
			properties.CopyAllValuesTo(buffer);
			buffer.GetValue(LogFileProperties.StartTimestamp).Should().Be(new DateTime(2017, 12, 29, 13, 3, 0));
			buffer.GetValue(LogFileProperties.EmptyReason).Should().Be(LogFileProperties.EmptyReason.DefaultValue);
		}
	}
}