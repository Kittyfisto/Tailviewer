using System.Net;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.PluginRepository.Applications;

namespace Tailviewer.PluginRepository.Tests
{
	[TestFixture]
	public sealed class IpEndPointExtensionsTest
	{
		[Test]
		public void TestParseIpv6()
		{
			IPEndPointExtensions.Parse("[::]:80")
			                    .Should().Be(new IPEndPoint(IPAddress.IPv6Any, 80));
		}

		[Test]
		public void TestParseIpv4()
		{
			IPEndPointExtensions.Parse("1.2.3.4:5342")
			                    .Should().Be(new IPEndPoint(IPAddress.Parse("1.2.3.4"), 5342));
		}
	}
}
