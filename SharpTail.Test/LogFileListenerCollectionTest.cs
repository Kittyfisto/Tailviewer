using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SharpTail.BusinessLogic;

namespace SharpTail.Test
{
	[TestFixture]
	public sealed class LogFileListenerCollectionTest
	{
		[Test]
		[Description("Verifies that AddListener may be called multiple times, but if it is, then events aren't fired multiple times for each invocation")]
		public void TestAddListener1()
		{
			var collection = new LogFileListenerCollection();
			var listener = new Mock<ILogFileListener>();
			int invoked = 0;
			listener.Setup(x => x.OnLogFileModified(It.IsAny<LogFileSection>()))
			        .Callback(() => ++invoked);

			collection.AddListener(listener.Object, TimeSpan.FromSeconds(1), 10);
			new Action(() => collection.AddListener(listener.Object, TimeSpan.FromSeconds(1), 10)).ShouldNotThrow();

			collection.OnLineRead(9);
			invoked.Should().Be(1, "Because even though we added the listener twice, it should never be invoked twice");
		}
	}
}