using System;
using System.Collections.Generic;
using FluentAssertions;
using Metrolib;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Properties
{
	[TestFixture]
	public abstract class AbstractPropertiesBufferTest
	{
		protected abstract IPropertiesBuffer Create(params KeyValuePair<IReadOnlyPropertyDescriptor, object>[] properties);

		[Test]
		public void TestDebuggerVisualization1()
		{
			var properties = Create();
			var view = new PropertiesBufferDebuggerVisualization(properties);
			view.Items.Should().BeEmpty();
		}

		[Test]
		public void TestDebuggerVisualization2()
		{
			var properties = Create(
			                        new KeyValuePair<IReadOnlyPropertyDescriptor, object>(Core.Properties.Created, new DateTime(2017, 12, 29, 12, 50, 0)),
			                        new KeyValuePair<IReadOnlyPropertyDescriptor, object>(Core.Properties.Size, Size.FromGigabytes(1))
			                        );
			var view = new PropertiesBufferDebuggerVisualization(properties);
			var items = view.Items;
			view.Items.Should().HaveCount(2);
			items.Should().ContainKey(Core.Properties.Created);
			items[Core.Properties.Created].Should().Be(new DateTime(2017, 12, 29, 12, 50, 0));
			items.Should().ContainKey(Core.Properties.Size);
			items[Core.Properties.Size].Should().Be(Size.FromGigabytes(1));
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
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(Core.Properties.EmptyReason, ErrorFlags.SourceCannotBeAccessed));
			DateTime? lastModified;
			properties.TryGetValue(Core.Properties.LastModified, out lastModified).Should().BeFalse();
			lastModified.Should().Be(Core.Properties.LastModified.DefaultValue);
		}

		[Test]
		public void TestGetValue1()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(Core.Properties.Created, new DateTime(2017, 12, 29, 13, 0, 0)));
			properties.GetValue(Core.Properties.Created).Should().Be(new DateTime(2017, 12, 29, 13, 0, 0));
		}

		[Test]
		public void TestSetValue1()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(Core.Properties.Created, new DateTime(2017, 12, 29, 13, 1, 0)));
			properties.SetValue(Core.Properties.Created, new DateTime(2017, 12, 29, 13, 2, 0));
			properties.GetValue(Core.Properties.Created).Should().Be(new DateTime(2017, 12, 29, 13, 2, 0));
		}

		[Test]
		[Description("Verifies that the non-generic overload works as well")]
		public void TestSetValue2()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(Core.Properties.Created, new DateTime(2017, 12, 29, 13, 1, 0)));
			properties.SetValue((IReadOnlyPropertyDescriptor)Core.Properties.Created, new DateTime(2017, 12, 29, 13, 2, 0));
			properties.GetValue(Core.Properties.Created).Should().Be(new DateTime(2017, 12, 29, 13, 2, 0));
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
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(Core.Properties.Created, new DateTime(2017, 12, 29, 13, 1, 0)),
			                        new KeyValuePair<IReadOnlyPropertyDescriptor, object>(Core.Properties.StartTimestamp, new DateTime(2017, 12, 29, 13, 3, 0)));
			new Action(() => properties.CopyAllValuesTo(null)).Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestCopyAllValuesTo2()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(Core.Properties.Created, new DateTime(2017, 12, 29, 13, 1, 0)),
			                        new KeyValuePair<IReadOnlyPropertyDescriptor, object>(Core.Properties.StartTimestamp, new DateTime(2017, 12, 29, 13, 3, 0)));

			var buffer = new PropertiesBufferList();
			properties.CopyAllValuesTo(buffer);
			buffer.Properties.Should().Equal(properties.Properties);
			buffer.GetValue(Core.Properties.Created).Should().Be(new DateTime(2017, 12, 29, 13, 1, 0));
			buffer.GetValue(Core.Properties.StartTimestamp).Should().Be(new DateTime(2017, 12, 29, 13, 3, 0));
		}

		[Test]
		[Description("Verifies that it's possible to retrieve a subset of values from a properties object")]
		public void TestCopyAllValuesTo3()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(Core.Properties.Created, new DateTime(2017, 12, 29, 13, 1, 0)),
			                    new KeyValuePair<IReadOnlyPropertyDescriptor, object>(Core.Properties.StartTimestamp, new DateTime(2017, 12, 29, 13, 3, 0)));

			var buffer = new PropertiesBufferList();
			properties.Except(Core.Properties.Minimum).CopyAllValuesTo(buffer);
			buffer.Properties.Should().BeEmpty();
		}

		[Test]
		[Description("Verifies that it's possible to retrieve a subset of values from a properties object")]
		public void TestCopyAllValuesTo4()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(Core.Properties.Created, new DateTime(2017, 12, 29, 13, 1, 0)),
			                        new KeyValuePair<IReadOnlyPropertyDescriptor, object>(Core.Properties.StartTimestamp, new DateTime(2017, 12, 29, 13, 3, 0)));

			var buffer = new PropertiesBufferList(Core.Properties.StartTimestamp);
			properties.CopyAllValuesTo(buffer);
			buffer.GetValue(Core.Properties.StartTimestamp).Should().Be(new DateTime(2017, 12, 29, 13, 3, 0));
		}

		[Test]
		[Description("Verifies that accessing non existing properties is allowed and returns their default value")]
		public void TestCopyAllValuesTo5()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(Core.Properties.Created, new DateTime(2017, 12, 29, 13, 1, 0)),
			                        new KeyValuePair<IReadOnlyPropertyDescriptor, object>(Core.Properties.StartTimestamp, new DateTime(2017, 12, 29, 13, 3, 0)));

			var buffer = new PropertiesBufferList(Core.Properties.StartTimestamp, Core.Properties.EmptyReason);
			properties.CopyAllValuesTo(buffer);
			buffer.GetValue(Core.Properties.StartTimestamp).Should().Be(new DateTime(2017, 12, 29, 13, 3, 0));
			buffer.GetValue(Core.Properties.EmptyReason).Should().Be(Core.Properties.EmptyReason.DefaultValue);
		}
	}
}