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