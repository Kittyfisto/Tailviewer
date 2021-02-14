using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogFilePropertyListTest
		: AbstractLogFilePropertiesTest
	{
		[Test]
		public void TestConstruction()
		{
			var properties = new LogFilePropertyList(LogFileProperties.Created);
			properties.GetValue(LogFileProperties.Created).Should().Be(LogFileProperties.Created.DefaultValue);
		}

		[Test]
		public void TestAdd()
		{
			var properties = new LogFilePropertyList();
			properties.Properties.Should().BeEmpty();

			properties.Add(LogFileProperties.Created);
			properties.Properties.Should().Equal(new object[]{LogFileProperties.Created});

			properties.SetValue(LogFileProperties.Created, new DateTime(2021, 02, 14, 12, 13, 01));
			new Action(()=> properties.Add(LogFileProperties.Created)).Should().NotThrow("because adding properties again should be tolerate and just not do anything");
			properties.Properties.Should().Equal(new object[] {LogFileProperties.Created});
			properties.GetValue(LogFileProperties.Created).Should().Be(new DateTime(2021, 02, 14, 12, 13, 01));
		}

		[Test]
		[Description("Verifies that values can be overwritten")]
		public void TestSetValue5()
		{
			var properties = new LogFilePropertyList();
			properties.SetValue(LogFileProperties.EmptyReason, ErrorFlags.SourceDoesNotExist);
			properties.GetValue(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.SourceDoesNotExist);

			properties.SetValue(LogFileProperties.EmptyReason, ErrorFlags.SourceCannotBeAccessed);
			properties.GetValue(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.SourceCannotBeAccessed);
		}

		[Test]
		[Description("Verifies that values can be overwritten")]
		public void TestSetValue6()
		{
			var properties = new LogFilePropertyList();
			properties.SetValue((ILogFilePropertyDescriptor)LogFileProperties.EmptyReason, ErrorFlags.SourceDoesNotExist);
			properties.GetValue(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.SourceDoesNotExist);

			properties.SetValue((ILogFilePropertyDescriptor)LogFileProperties.EmptyReason, ErrorFlags.SourceCannotBeAccessed);
			properties.GetValue(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.SourceCannotBeAccessed);
		}

		[Test]
		[Description("Verifies that values are reset to the default value as specified by their property")]
		public void TestReset()
		{
			var properties = new LogFilePropertyList();
			properties.SetValue(LogFileProperties.EmptyReason, ErrorFlags.SourceCannotBeAccessed);
			properties.SetValue(LogFileProperties.PercentageProcessed, Percentage.Of(50, 100));

			properties.Reset();
			properties.GetValue(LogFileProperties.EmptyReason).Should().Be(LogFileProperties.EmptyReason.DefaultValue);
			properties.GetValue(LogFileProperties.PercentageProcessed).Should().Be(Percentage.Zero);
		}

		[Test]
		[Description("Verifies that all properties are removed from the list")]
		public void TestClear()
		{
			var properties = new LogFilePropertyList();
			properties.SetValue(LogFileProperties.EmptyReason, ErrorFlags.SourceCannotBeAccessed);
			properties.SetValue(LogFileProperties.PercentageProcessed, Percentage.Of(50, 100));

			properties.Clear();
			properties.Properties.Should().BeEmpty();
			properties.GetValue(LogFileProperties.EmptyReason).Should().Be(LogFileProperties.EmptyReason.DefaultValue);
			properties.GetValue(LogFileProperties.PercentageProcessed).Should().Be(LogFileProperties.PercentageProcessed.DefaultValue);

			properties.TryGetValue(LogFileProperties.EmptyReason, out _).Should().BeFalse();
			properties.TryGetValue(LogFileProperties.PercentageProcessed, out _).Should().BeFalse();
		}

		protected override ILogFileProperties Create(params KeyValuePair<ILogFilePropertyDescriptor, object>[] properties)
		{
			var list = new LogFilePropertyList(properties.Select(x => x.Key).ToArray());
			foreach (var pair in properties)
			{
				list.SetValue(pair.Key, pair.Value);
			}

			return list;
		}
	}
}