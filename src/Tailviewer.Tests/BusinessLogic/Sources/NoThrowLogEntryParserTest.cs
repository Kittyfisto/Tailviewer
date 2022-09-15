using System;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using FluentAssertions;
using Tailviewer.Api;
using Tailviewer.BusinessLogic.Sources;

namespace Tailviewer.Tests.BusinessLogic.Sources
{
	[TestFixture]
	public sealed class NoThrowLogEntryParserTest
	{
		[Test]
		public void TestColumnsShallNotReturnNull()
		{
			var inner = new Mock<ILogEntryParser>();
			inner.Setup(x => x.Columns).Returns((IEnumerable < IColumnDescriptor > )null);
			var parser = new NoThrowLogEntryParser(inner.Object);

			var columns = parser.Columns;
			columns.Should().NotBeNull();
			columns.Should().BeEmpty();
		}

		[Test]
		public void TestColumnsShallNotThrow()
		{
			var inner = new Mock<ILogEntryParser>();
			inner.Setup(x => x.Columns).Throws<NullReferenceException>();
			var parser = new NoThrowLogEntryParser(inner.Object);

			var columns = parser.Columns;
			columns.Should().NotBeNull();
			columns.Should().BeEmpty();
		}
	}
}
