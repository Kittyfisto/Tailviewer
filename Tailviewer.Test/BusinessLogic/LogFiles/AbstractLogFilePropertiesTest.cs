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
		protected abstract ILogFileProperties Create(params KeyValuePair<ILogFilePropertyDescriptor, object>[] properties);

		[Test]
		public void TestDebuggerVisualization1()
		{
			var properties = Create();
			var view = new LogFilePropertiesView(properties);
			view.Items.Should().BeEmpty();
		}

		[Test]
		public void TestDebuggerVisualization2()
		{
			var properties = Create(
			                        new KeyValuePair<ILogFilePropertyDescriptor, object>(LogFileProperties.Created, new DateTime(2017, 12, 29, 12, 50, 0)),
			                        new KeyValuePair<ILogFilePropertyDescriptor, object>(LogFileProperties.Size, Size.FromGigabytes(1))
			                        );
			var view = new LogFilePropertiesView(properties);
			view.Items.Should().HaveCount(2);
			view.Items[0].PropertyDescriptor.Should().Be(LogFileProperties.Created);
			view.Items[0].Value.Should().Be(new DateTime(2017, 12, 29, 12, 50, 0));
			view.Items[1].PropertyDescriptor.Should().Be(LogFileProperties.Size);
			view.Items[1].Value.Should().Be(Size.FromGigabytes(1));
		}

		[Test]
		public void TestTryGetValue1()
		{
			var properties = Create();
			object unused;
			new Action(() => properties.TryGetValue(null, out unused))
				.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void TestTryGetValue2()
		{
			var properties = Create();
			string unused;
			new Action(() => properties.TryGetValue(null, out unused))
				.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void TestTryGetValue3()
		{
			var properties = Create(new KeyValuePair<ILogFilePropertyDescriptor, object>(LogFileProperties.EmptyReason, ErrorFlags.SourceCannotBeAccessed));
			DateTime? lastModified;
			properties.TryGetValue(LogFileProperties.LastModified, out lastModified).Should().BeFalse();
			lastModified.Should().Be(LogFileProperties.LastModified.DefaultValue);
		}

		[Test]
		public void TestGetValue1()
		{
			var properties = Create(new KeyValuePair<ILogFilePropertyDescriptor, object>(LogFileProperties.Created, new DateTime(2017, 12, 29, 13, 0, 0)));
			properties.GetValue(LogFileProperties.Created).Should().Be(new DateTime(2017, 12, 29, 13, 0, 0));
		}

		[Test]
		public void TestSetValue1()
		{
			var properties = Create(new KeyValuePair<ILogFilePropertyDescriptor, object>(LogFileProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)));
			properties.SetValue(LogFileProperties.Created, new DateTime(2017, 12, 29, 13, 2, 0));
			properties.GetValue(LogFileProperties.Created).Should().Be(new DateTime(2017, 12, 29, 13, 2, 0));
		}

		[Test]
		[Description("Verifies that the non-generic overload works as well")]
		public void TestSetValue2()
		{
			var properties = Create(new KeyValuePair<ILogFilePropertyDescriptor, object>(LogFileProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)));
			properties.SetValue((ILogFilePropertyDescriptor)LogFileProperties.Created, new DateTime(2017, 12, 29, 13, 2, 0));
			properties.GetValue(LogFileProperties.Created).Should().Be(new DateTime(2017, 12, 29, 13, 2, 0));
		}

		[Test]
		public void TestSetValue3()
		{
			var properties = Create();
			new Action(() => properties.SetValue(null, new DateTime(2017, 12, 29, 13, 2, 0)))
				.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		[Description("Verifies that the non-generic overload throws")]
		public void TestSetValue4()
		{
			var properties = Create();
			new Action(() => properties.SetValue((ILogFilePropertyDescriptor) null, new DateTime(2017, 12, 29, 13, 2, 0)))
				.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void TestGetValues1()
		{
			var properties = Create(new KeyValuePair<ILogFilePropertyDescriptor, object>(LogFileProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)),
			                        new KeyValuePair<ILogFilePropertyDescriptor, object>(LogFileProperties.StartTimestamp, new DateTime(2017, 12, 29, 13, 3, 0)));
			new Action(() => properties.GetValues(null)).ShouldThrow<ArgumentNullException>();
		}

		[Test]
		[Description("Verifies that it's possible to retrieve a subset of values from a properties object")]
		public void TestGetValues2()
		{
			var properties = Create(new KeyValuePair<ILogFilePropertyDescriptor, object>(LogFileProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)),
			                    new KeyValuePair<ILogFilePropertyDescriptor, object>(LogFileProperties.StartTimestamp, new DateTime(2017, 12, 29, 13, 3, 0)));

			var buffer = new LogFilePropertyList();
			properties.GetValues(buffer);
			buffer.Properties.Should().BeEmpty();
		}

		[Test]
		[Description("Verifies that it's possible to retrieve a subset of values from a properties object")]
		public void TestGetValues3()
		{
			var properties = Create(new KeyValuePair<ILogFilePropertyDescriptor, object>(LogFileProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)),
			                        new KeyValuePair<ILogFilePropertyDescriptor, object>(LogFileProperties.StartTimestamp, new DateTime(2017, 12, 29, 13, 3, 0)));

			var buffer = new LogFilePropertyList(LogFileProperties.StartTimestamp);
			properties.GetValues(buffer);
			buffer.GetValue(LogFileProperties.StartTimestamp).Should().Be(new DateTime(2017, 12, 29, 13, 3, 0));
		}

		[Test]
		[Description("Verifies that accessing non existing properties is allowed and returns their default value")]
		public void TestGetValues4()
		{
			var properties = Create(new KeyValuePair<ILogFilePropertyDescriptor, object>(LogFileProperties.Created, new DateTime(2017, 12, 29, 13, 1, 0)),
			                        new KeyValuePair<ILogFilePropertyDescriptor, object>(LogFileProperties.StartTimestamp, new DateTime(2017, 12, 29, 13, 3, 0)));

			var buffer = new LogFilePropertyList(LogFileProperties.StartTimestamp, LogFileProperties.EmptyReason);
			properties.GetValues(buffer);
			buffer.GetValue(LogFileProperties.StartTimestamp).Should().Be(new DateTime(2017, 12, 29, 13, 3, 0));
			buffer.GetValue(LogFileProperties.EmptyReason).Should().Be(LogFileProperties.EmptyReason.DefaultValue);
		}
	}
}