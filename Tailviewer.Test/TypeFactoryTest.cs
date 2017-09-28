using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;

namespace Tailviewer.Test
{
	[TestFixture]
	public sealed class TypeFactoryTest
	{
		[Test]
		public void TestTryCreate1()
		{
			var factory = new TypeFactory(new KeyValuePair<string, Type>[0]);
			factory.TryCreateNew(null).Should().BeNull();
		}
	}
}