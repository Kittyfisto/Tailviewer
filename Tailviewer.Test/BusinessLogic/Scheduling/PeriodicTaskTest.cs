using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Tailviewer.Test.BusinessLogic.Scheduling
{
	[TestFixture]
	public sealed class PeriodicTaskTest
	{
		[Test]
		public void TestCtor()
		{
			var task = new PeriodicTask(42, () => { }, TimeSpan.FromSeconds(1));
			task.Id.Should().Be(42);
			task.IsRemoved.Should().BeFalse();
			task.LastInvocation.Should().Be(DateTime.MinValue);
		}
	}
}