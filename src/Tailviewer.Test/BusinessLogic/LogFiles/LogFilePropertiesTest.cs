using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Properties;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogFilePropertiesTest
	{
		[Test]
		public void TestWellKnown()
		{
			GeneralProperties.LogEntryCount.Should().NotBeNull();
			GeneralProperties.Name.Should().NotBeNull();
			GeneralProperties.StartTimestamp.Should().NotBeNull();
			GeneralProperties.EndTimestamp.Should().NotBeNull();
			GeneralProperties.Duration.Should().NotBeNull();
			GeneralProperties.LastModified.Should().NotBeNull();
			GeneralProperties.StartTimestamp.Should().NotBeNull();
			GeneralProperties.Created.Should().NotBeNull();
			GeneralProperties.Size.Should().NotBeNull();
			GeneralProperties.PercentageProcessed.Should().NotBeNull();
			GeneralProperties.EmptyReason.Should().NotBeNull();
			GeneralProperties.Format.Should().NotBeNull();
			GeneralProperties.FormatDetectionCertainty.Should().NotBeNull();
			GeneralProperties.Encoding.Should().NotBeNull();
		}

		[Test]
		public void TestCombineWithMinimum1()
		{
			GeneralProperties.CombineWithMinimum(null).Should().Equal(GeneralProperties.Minimum);
		}

		[Test]
		public void TestCombineWithMinimum2()
		{
			GeneralProperties.CombineWithMinimum(GeneralProperties.Minimum).Should().Equal(GeneralProperties.Minimum);
		}

		[Test]
		public void TestCombineWithMinimum3()
		{
			var property = new Mock<IReadOnlyPropertyDescriptor>().Object;
			GeneralProperties.CombineWithMinimum(property).Should().Equal(GeneralProperties.Minimum
			                                                                               .Concat(new [] {property}));
		}
	}
}