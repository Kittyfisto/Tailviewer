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
			LogFileProperties.LogEntryCount.Should().NotBeNull();
			LogFileProperties.Name.Should().NotBeNull();
			LogFileProperties.StartTimestamp.Should().NotBeNull();
			LogFileProperties.EndTimestamp.Should().NotBeNull();
			LogFileProperties.Duration.Should().NotBeNull();
			LogFileProperties.LastModified.Should().NotBeNull();
			LogFileProperties.StartTimestamp.Should().NotBeNull();
			LogFileProperties.Created.Should().NotBeNull();
			LogFileProperties.Size.Should().NotBeNull();
			LogFileProperties.PercentageProcessed.Should().NotBeNull();
			LogFileProperties.EmptyReason.Should().NotBeNull();
			LogFileProperties.Format.Should().NotBeNull();
			LogFileProperties.FormatDetectionCertainty.Should().NotBeNull();
			LogFileProperties.Encoding.Should().NotBeNull();
		}

		[Test]
		public void TestCombineWithMinimum1()
		{
			LogFileProperties.CombineWithMinimum(null).Should().Equal(LogFileProperties.Minimum);
		}

		[Test]
		public void TestCombineWithMinimum2()
		{
			LogFileProperties.CombineWithMinimum(LogFileProperties.Minimum).Should().Equal(LogFileProperties.Minimum);
		}

		[Test]
		public void TestCombineWithMinimum3()
		{
			var property = new Mock<ILogFilePropertyDescriptor>().Object;
			LogFileProperties.CombineWithMinimum(property).Should().Equal(LogFileProperties.Minimum
			                                                                               .Concat(new [] {property}));
		}
	}
}