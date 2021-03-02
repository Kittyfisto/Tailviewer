using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core;

namespace Tailviewer.Tests
{
	[TestFixture]
	public sealed class ServiceContainerTest
	{
		[Test]
		public void TestRetrieveRegisteredService()
		{
			var service = new Mock<ITypeFactory>().Object;

			var container = new ServiceContainer();
			container.RegisterInstance<ITypeFactory>(service);
			container.Retrieve<ITypeFactory>().Should().BeSameAs(service);
			container.TryRetrieve(out ITypeFactory service2).Should().BeTrue();
			service2.Should().BeSameAs(service);
		}

		[Test]
		public void TestRetrieveOverwrittenService()
		{
			var service1 = new Mock<ITypeFactory>().Object;
			var service2 = new Mock<ITypeFactory>().Object;

			var container = new ServiceContainer();
			container.RegisterInstance<ITypeFactory>(service1);
			container.RegisterInstance<ITypeFactory>(service2);

			var retrievedService = container.Retrieve<ITypeFactory>();
			retrievedService.Should().NotBeSameAs(service1);
			retrievedService.Should().BeSameAs(service2);
		}

		[Test]
		public void TestRetrieveUnregisteredService()
		{
			var container = new ServiceContainer();
			new Action(() => container.Retrieve<ITypeFactory>())
				.Should().Throw<ArgumentException>()
				.WithMessage("No service has been registered with this container which implements Tailviewer.Api.ITypeFactory");

			container.TryRetrieve(out ITypeFactory service).Should().BeFalse();
			service.Should().BeNull();
		}
	}
}
