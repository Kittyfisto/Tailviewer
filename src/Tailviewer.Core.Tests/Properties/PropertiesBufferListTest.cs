using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Properties
{
	[TestFixture]
	public sealed class PropertiesBufferListTest
		: AbstractPropertiesBufferTest
	{
		[Test]
		public void TestConstruction()
		{
			var properties = new PropertiesBufferList(Core.Properties.Created);
			properties.GetValue(Core.Properties.Created).Should().Be(Core.Properties.Created.DefaultValue);
		}

		[Test]
		public void TestAdd()
		{
			var properties = new PropertiesBufferList();
			properties.Properties.Should().BeEmpty();

			properties.Add(Core.Properties.Created);
			properties.Properties.Should().Equal(new object[]{Core.Properties.Created});

			properties.SetValue(Core.Properties.Created, new DateTime(2021, 02, 14, 12, 13, 01));
			new Action(()=> properties.Add(Core.Properties.Created)).Should().NotThrow("because adding properties again should be tolerate and just not do anything");
			properties.Properties.Should().Equal(new object[] {Core.Properties.Created});
			properties.GetValue(Core.Properties.Created).Should().Be(new DateTime(2021, 02, 14, 12, 13, 01));
		}

		[Test]
		[Description("Verifies that values can be overwritten")]
		public void TestSetValue5()
		{
			var properties = new PropertiesBufferList();
			var sourceDoesNotExist = new SourceDoesNotExist("dawdaw.txt");
			properties.SetValue(Core.Properties.EmptyReason, sourceDoesNotExist);
			properties.GetValue(Core.Properties.EmptyReason).Should().Be(sourceDoesNotExist);

			var sourceCannotBeAccessed = new SourceDoesNotExist("wdawdwaw.txt");
			properties.SetValue(Core.Properties.EmptyReason, sourceCannotBeAccessed);
			properties.GetValue(Core.Properties.EmptyReason).Should().Be(sourceCannotBeAccessed);
		}

		[Test]
		[Description("Verifies that values can be overwritten")]
		public void TestSetValue6()
		{
			var properties = new PropertiesBufferList();
			var sourceDoesNotExist = new SourceDoesNotExist("dawdaw.txt");
			properties.SetValue((IReadOnlyPropertyDescriptor)Core.Properties.EmptyReason, sourceDoesNotExist);
			properties.GetValue(Core.Properties.EmptyReason).Should().Be(sourceDoesNotExist);

			var sourceCannotBeAccessed = new SourceDoesNotExist("wdawdwaw.txt");
			properties.SetValue((IReadOnlyPropertyDescriptor)Core.Properties.EmptyReason, sourceCannotBeAccessed);
			properties.GetValue(Core.Properties.EmptyReason).Should().Be(sourceCannotBeAccessed);
		}

		[Test]
		[Description("Verifies that values are reset to the default value as specified by their property")]
		public void TestSetToDefault()
		{
			var properties = new PropertiesBufferList();
			var sourceCannotBeAccessed = new SourceDoesNotExist("wdawdwaw.txt");
			properties.SetValue(Core.Properties.EmptyReason, sourceCannotBeAccessed);
			properties.SetValue(Core.Properties.PercentageProcessed, Percentage.Of(50, 100));

			properties.SetToDefault();
			properties.GetValue(Core.Properties.EmptyReason).Should().Be(Core.Properties.EmptyReason.DefaultValue);
			properties.GetValue(Core.Properties.PercentageProcessed).Should().Be(Percentage.Zero);
		}

		[Test]
		[Description("Verifies that only the properties specified are reset to default")]
		public void TestSetToDefaultPartial()
		{
			var properties = new PropertiesBufferList();
			var sourceCannotBeAccessed = new SourceDoesNotExist("wdawdwaw.txt");
			properties.SetValue(Core.Properties.EmptyReason, sourceCannotBeAccessed);
			properties.SetValue(Core.Properties.PercentageProcessed, Percentage.Of(50, 100));

			properties.SetToDefault(new []{Core.Properties.PercentageProcessed});
			properties.GetValue(Core.Properties.EmptyReason).Should().Be(sourceCannotBeAccessed, "because only the PercentageProcessed property may have been reset");
			properties.GetValue(Core.Properties.PercentageProcessed).Should().Be(Core.Properties.PercentageProcessed.DefaultValue);
		}

		[Test]
		[Description("Verifies that only the properties specified are reset to default")]
		public void TestSetToDefaultNull()
		{
			var properties = new PropertiesBufferList();
			var sourceCannotBeAccessed = new SourceDoesNotExist("wdawdwaw.txt");
			properties.SetValue(Core.Properties.EmptyReason, sourceCannotBeAccessed);
			properties.SetValue(Core.Properties.PercentageProcessed, Percentage.Of(50, 100));

			new Action(() => properties.SetToDefault(null)).Should().Throw<ArgumentNullException>();
			properties.GetValue(Core.Properties.EmptyReason).Should().Be(sourceCannotBeAccessed);
			properties.GetValue(Core.Properties.PercentageProcessed).Should().Be(Percentage.Of(50, 100));
		}

		[Test]
		[Description("Verifies that all properties are removed from the list")]
		public void TestClear()
		{
			var properties = new PropertiesBufferList();
			var sourceCannotBeAccessed = new SourceDoesNotExist("wdawdwaw.txt");
			properties.SetValue(Core.Properties.EmptyReason, sourceCannotBeAccessed);
			properties.SetValue(Core.Properties.PercentageProcessed, Percentage.Of(50, 100));

			properties.Clear();
			properties.Properties.Should().BeEmpty();
			properties.GetValue(Core.Properties.EmptyReason).Should().Be(Core.Properties.EmptyReason.DefaultValue);
			properties.GetValue(Core.Properties.PercentageProcessed).Should().Be(Core.Properties.PercentageProcessed.DefaultValue);

			properties.TryGetValue(Core.Properties.EmptyReason, out _).Should().BeFalse();
			properties.TryGetValue(Core.Properties.PercentageProcessed, out _).Should().BeFalse();
		}

		protected override IPropertiesBuffer Create(params KeyValuePair<IReadOnlyPropertyDescriptor, object>[] properties)
		{
			var list = new PropertiesBufferList(properties.Select(x => x.Key).ToArray());
			foreach (var pair in properties)
			{
				list.SetValue(pair.Key, pair.Value);
			}

			return list;
		}
	}
}