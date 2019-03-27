using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public abstract class AbstractReadOnlyLogEntryTest
	{
		protected abstract IReadOnlyLogEntry CreateEmpty();

		[Test]
		public void TestTryGetValue1()
		{
			var column = new Mock<ILogFileColumn<int>>();
			column.Setup(x => x.DefaultValue).Returns(42);

			var entry = CreateEmpty();
			int value;
			entry.TryGetValue(column.Object, out value).Should().BeFalse("because that column hasn't been added");
			value.Should().Be(42);
		}

		[Test]
		public void TestTryGetValue2()
		{
			var column = new Mock<ILogFileColumn>();
			column.Setup(x => x.DefaultValue).Returns(42);

			var entry = CreateEmpty();
			object value;
			entry.TryGetValue(column.Object, out value).Should().BeFalse("because that column hasn't been added");
			value.Should().Be(42);
		}

		[Test]
		public void TestToStringEmpty()
		{
			var entry = CreateEmpty();
			entry.ToString().Should().Be("");
		}
	}
}