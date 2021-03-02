using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;

namespace Tailviewer.Tests
{
	[TestFixture]
	public sealed class TypeFactoryTest
	{
		[Test]
		public void TestTryCreateUnregisteredType()
		{
			var factory = new TypeFactory(new KeyValuePair<string, Type>[0]);
			factory.TryCreateNew(null).Should().BeNull();
		}

		[Test]
		public void TestTryCreateRegisteredType()
		{
			var factory = new TypeFactory();

			var name = "dwaawkdwanwaw";
			factory.Add(name, typeof(QuickFilterId));
			factory.TryCreateNew(name).Should().BeOfType<QuickFilterId>();
		}

		[Test]
		public void TestTryGetTypeName()
		{
			var factory = new TypeFactory();

			var name = "MyCustomQuickFilterId";
			factory.Add(name, typeof(QuickFilterId));
			factory.TryGetTypeName(typeof(QuickFilterId)).Should().Be(name);
		}
	}
}