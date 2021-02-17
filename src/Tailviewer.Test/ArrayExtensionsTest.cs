using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;

namespace Tailviewer.Test
{
	[TestFixture]
	public sealed class ArrayExtensionsTest
	{
		[Test]
		public void TestStartsWith()
		{
			ArrayExtensions.StartsWith(new byte[] {1}, new byte[] {2}).Should().BeFalse();
			ArrayExtensions.StartsWith(new byte[] {1}, new byte[] {1, 2}).Should().BeFalse();
			ArrayExtensions.StartsWith(new byte[] {2}, new byte[] {1, 2}).Should().BeFalse();
			ArrayExtensions.StartsWith(new byte[] {2, 1}, new byte[] {2}).Should().BeTrue();
			ArrayExtensions.StartsWith(new byte[] {2}, new byte[] {2}).Should().BeTrue();
		}
	}
}
