using System;
using System.Collections.Generic;
using FluentAssertions;
using Metrolib;
using NUnit.Framework;
using Tailviewer.Core.Properties;

namespace Tailviewer.Test.BusinessLogic.Properties
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
			                        new KeyValuePair<IReadOnlyPropertyDescriptor, object>(GeneralProperties.Created, new DateTime(2017, 12, 29, 12, 50, 0)),
			                        new KeyValuePair<IReadOnlyPropertyDescriptor, object>(GeneralProperties.Size, Size.FromGigabytes(1))
			                        );
			var view = new PropertiesBufferDebuggerVisualization(properties);
			var items = view.Items;
			view.Items.Should().HaveCount(2);
			items.Should().ContainKey(GeneralProperties.Created);
			items[GeneralProperties.Created].Should().Be(new DateTime(2017, 12, 29, 12, 50, 0));
			items.Should().ContainKey(GeneralProperties.Size);
			items[GeneralProperties.Size].Should().Be(Size.FromGigabytes(1));
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
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(GeneralProperties.EmptyReason, ErrorFlags.SourceCannotBeAccessed));
			DateTime? lastModified;
			properties.TryGetValue(GeneralProperties.LastModified, out lastModified).Should().BeFalse();
			lastModified.Should().Be(GeneralProperties.LastModified.DefaultValue);
		}

		[Test]
		public void TestGetValue1()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(GeneralProperties.Created, new DateTime(2017, 12, 29, 13, 0, 0)));
			properties.GetValue(GeneralProperties.Created).Should().Be(new DateTime(2017, 12, 29, 13, 0, 0));
		}

		[Test]
		public void TestSetValue1()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(GeneralProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)));
			properties.SetValue(GeneralProperties.Created, new DateTime(2017, 12, 29, 13, 2, 0));
			properties.GetValue(GeneralProperties.Created).Should().Be(new DateTime(2017, 12, 29, 13, 2, 0));
		}

		[Test]
		[Description("Verifies that the non-generic overload works as well")]
		public void TestSetValue2()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(GeneralProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)));
			properties.SetValue((IReadOnlyPropertyDescriptor)GeneralProperties.Created, new DateTime(2017, 12, 29, 13, 2, 0));
			properties.GetValue(GeneralProperties.Created).Should().Be(new DateTime(2017, 12, 29, 13, 2, 0));
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
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(GeneralProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)),
			                        new KeyValuePair<IReadOnlyPropertyDescriptor, object>(GeneralProperties.StartTimestamp, new DateTime(2017, 12, 29, 13, 3, 0)));
			new Action(() => properties.CopyAllValuesTo(null)).Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestCopyAllValuesTo2()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(GeneralProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)),
			                        new KeyValuePair<IReadOnlyPropertyDescriptor, object>(GeneralProperties.StartTimestamp, new DateTime(2017, 12, 29, 13, 3, 0)));

			var buffer = new PropertiesBufferList();
			properties.CopyAllValuesTo(buffer);
			buffer.Properties.Should().Equal(properties.Properties);
			buffer.GetValue(GeneralProperties.Created).Should().Be(new DateTime(2017, 12, 29, 13, 1, 0));
			buffer.GetValue(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2017, 12, 29, 13, 3, 0));
		}

		[Test]
		[Description("Verifies that it's possible to retrieve a subset of values from a properties object")]
		public void TestCopyAllValuesTo3()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(GeneralProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)),
			                    new KeyValuePair<IReadOnlyPropertyDescriptor, object>(GeneralProperties.StartTimestamp, new DateTime(2017, 12, 29, 13, 3, 0)));

			var buffer = new PropertiesBufferList();
			properties.Except(GeneralProperties.Minimum).CopyAllValuesTo(buffer);
			buffer.Properties.Should().BeEmpty();
		}

		[Test]
		[Description("Verifies that it's possible to retrieve a subset of values from a properties object")]
		public void TestCopyAllValuesTo4()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(GeneralProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)),
			                        new KeyValuePair<IReadOnlyPropertyDescriptor, object>(GeneralProperties.StartTimestamp, new DateTime(2017, 12, 29, 13, 3, 0)));

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp);
			properties.CopyAllValuesTo(buffer);
			buffer.GetValue(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2017, 12, 29, 13, 3, 0));
		}

		[Test]
		[Description("Verifies that accessing non existing properties is allowed and returns their default value")]
		public void TestCopyAllValuesTo5()
		{
			var properties = Create(new KeyValuePair<IReadOnlyPropertyDescriptor, object>(GeneralProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)),
			                        new KeyValuePair<IReadOnlyPropertyDescriptor, object>(GeneralProperties.StartTimestamp, new DateTime(2017, 12, 29, 13, 3, 0)));

			var buffer = new PropertiesBufferList(GeneralProperties.StartTimestamp, GeneralProperties.EmptyReason);
			properties.CopyAllValuesTo(buffer);
			buffer.GetValue(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2017, 12, 29, 13, 3, 0));
			buffer.GetValue(GeneralProperties.EmptyReason).Should().Be(GeneralProperties.EmptyReason.DefaultValue);
		}
	}
}