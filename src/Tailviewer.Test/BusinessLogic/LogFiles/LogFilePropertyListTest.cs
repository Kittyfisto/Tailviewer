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
			var properties = new LogFilePropertyList(Properties.Created);
			properties.GetValue(Properties.Created).Should().Be(Properties.Created.DefaultValue);
		}

		[Test]
		public void TestAdd()
		{
			var properties = new LogFilePropertyList();
			properties.Properties.Should().BeEmpty();

			properties.Add(Properties.Created);
			properties.Properties.Should().Equal(new object[]{Properties.Created});

			properties.SetValue(Properties.Created, new DateTime(2021, 02, 14, 12, 13, 01));
			new Action(()=> properties.Add(Properties.Created)).Should().NotThrow("because adding properties again should be tolerate and just not do anything");
			properties.Properties.Should().Equal(new object[] {Properties.Created});
			properties.GetValue(Properties.Created).Should().Be(new DateTime(2021, 02, 14, 12, 13, 01));
		}

		[Test]
		[Description("Verifies that values can be overwritten")]
		public void TestSetValue5()
		{
			var properties = new LogFilePropertyList();
			properties.SetValue(Properties.EmptyReason, ErrorFlags.SourceDoesNotExist);
			properties.GetValue(Properties.EmptyReason).Should().Be(ErrorFlags.SourceDoesNotExist);

			properties.SetValue(Properties.EmptyReason, ErrorFlags.SourceCannotBeAccessed);
			properties.GetValue(Properties.EmptyReason).Should().Be(ErrorFlags.SourceCannotBeAccessed);
		}

		[Test]
		[Description("Verifies that values can be overwritten")]
		public void TestSetValue6()
		{
			var properties = new LogFilePropertyList();
			properties.SetValue((IReadOnlyPropertyDescriptor)Properties.EmptyReason, ErrorFlags.SourceDoesNotExist);
			properties.GetValue(Properties.EmptyReason).Should().Be(ErrorFlags.SourceDoesNotExist);

			properties.SetValue((IReadOnlyPropertyDescriptor)Properties.EmptyReason, ErrorFlags.SourceCannotBeAccessed);
			properties.GetValue(Properties.EmptyReason).Should().Be(ErrorFlags.SourceCannotBeAccessed);
		}

		[Test]
		[Description("Verifies that values are reset to the default value as specified by their property")]
		public void TestReset()
		{
			var properties = new LogFilePropertyList();
			properties.SetValue(Properties.EmptyReason, ErrorFlags.SourceCannotBeAccessed);
			properties.SetValue(Properties.PercentageProcessed, Percentage.Of(50, 100));

			properties.Reset();
			properties.GetValue(Properties.EmptyReason).Should().Be(Properties.EmptyReason.DefaultValue);
			properties.GetValue(Properties.PercentageProcessed).Should().Be(Percentage.Zero);
		}

		[Test]
		[Description("Verifies that all properties are removed from the list")]
		public void TestClear()
		{
			var properties = new LogFilePropertyList();
			properties.SetValue(Properties.EmptyReason, ErrorFlags.SourceCannotBeAccessed);
			properties.SetValue(Properties.PercentageProcessed, Percentage.Of(50, 100));

			properties.Clear();
			properties.Properties.Should().BeEmpty();
			properties.GetValue(Properties.EmptyReason).Should().Be(Properties.EmptyReason.DefaultValue);
			properties.GetValue(Properties.PercentageProcessed).Should().Be(Properties.PercentageProcessed.DefaultValue);

			properties.TryGetValue(Properties.EmptyReason, out _).Should().BeFalse();
			properties.TryGetValue(Properties.PercentageProcessed, out _).Should().BeFalse();
		}

		protected override ILogFileProperties Create(params KeyValuePair<IReadOnlyPropertyDescriptor, object>[] properties)
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