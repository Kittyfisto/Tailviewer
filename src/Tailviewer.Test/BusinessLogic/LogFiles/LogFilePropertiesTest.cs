using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogFilePropertiesTest
	{
		[Test]
		public void TestWellKnown()
		{
			Properties.LogEntryCount.Should().NotBeNull();
			Properties.Name.Should().NotBeNull();
			Properties.StartTimestamp.Should().NotBeNull();
			Properties.EndTimestamp.Should().NotBeNull();
			Properties.Duration.Should().NotBeNull();
			Properties.LastModified.Should().NotBeNull();
			Properties.StartTimestamp.Should().NotBeNull();
			Properties.Created.Should().NotBeNull();
			Properties.Size.Should().NotBeNull();
			Properties.PercentageProcessed.Should().NotBeNull();
			Properties.EmptyReason.Should().NotBeNull();
			Properties.Format.Should().NotBeNull();
			Properties.FormatDetectionCertainty.Should().NotBeNull();
			Properties.Encoding.Should().NotBeNull();
		}

		[Test]
		public void TestCombineWithMinimum1()
		{
			Properties.CombineWithMinimum(null).Should().Equal(Properties.Minimum);
		}

		[Test]
		public void TestCombineWithMinimum2()
		{
			Properties.CombineWithMinimum(Properties.Minimum).Should().Equal(Properties.Minimum);
		}

		[Test]
		public void TestCombineWithMinimum3()
		{
			var property = new Mock<IReadOnlyPropertyDescriptor>().Object;
			Properties.CombineWithMinimum(property).Should().Equal(Properties.Minimum
			                                                                               .Concat(new [] {property}));
		}
	}
}