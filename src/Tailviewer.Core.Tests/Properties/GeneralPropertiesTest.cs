using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Properties
{
	[TestFixture]
	public sealed class GeneralPropertiesTest
	{
		[Test]
		public void TestWellKnown()
		{
			Core.Properties.LogEntryCount.Should().NotBeNull();
			Core.Properties.Name.Should().NotBeNull();
			Core.Properties.StartTimestamp.Should().NotBeNull();
			Core.Properties.EndTimestamp.Should().NotBeNull();
			Core.Properties.Duration.Should().NotBeNull();
			Core.Properties.LastModified.Should().NotBeNull();
			Core.Properties.StartTimestamp.Should().NotBeNull();
			Core.Properties.Created.Should().NotBeNull();
			Core.Properties.Size.Should().NotBeNull();
			Core.Properties.PercentageProcessed.Should().NotBeNull();
			Core.Properties.EmptyReason.Should().NotBeNull();
			Core.Properties.Format.Should().NotBeNull();
			Core.Properties.FormatDetectionCertainty.Should().NotBeNull();
			TextProperties.AutoDetectedEncoding.Should().NotBeNull();
		}

		[Test]
		public void TestCombineWithMinimum1()
		{
			Core.Properties.CombineWithMinimum(null).Should().Equal(Core.Properties.Minimum);
		}

		[Test]
		public void TestCombineWithMinimum2()
		{
			Core.Properties.CombineWithMinimum(Core.Properties.Minimum).Should().Equal(Core.Properties.Minimum);
		}

		[Test]
		public void TestCombineWithMinimum3()
		{
			var property = new Mock<IReadOnlyPropertyDescriptor>().Object;
			Core.Properties.CombineWithMinimum(property).Should().Equal(Core.Properties.Minimum
			                                                                .Concat(new [] {property}));
		}
	}
}