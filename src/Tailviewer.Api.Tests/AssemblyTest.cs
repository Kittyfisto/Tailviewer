using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Tailviewer.Api.Tests
{
	[TestFixture]
	public sealed class AssemblyTest
	{
		[Test]
		public void TestNamespaces()
		{
			var exportedTypes = typeof(ILogSource).Assembly.ExportedTypes;
			exportedTypes.Should().NotBeEmpty("because the Tailviewer.Api assembly should export some types");

			var failingTypes = new List<Type>();
			var expectedNamespace = "Tailviewer.Api";
			foreach (var type in exportedTypes)
			{
				if (type.Namespace != expectedNamespace)
					failingTypes.Add(type);
			}

			if (failingTypes.Any())
			{
				Assert.Fail("The following types should be located directly in the {0} namespace but are not:\r\n{1}",
				            "Tailviewer.Api",
				            string.Join("\r\n", failingTypes.Select(x => x.FullName)));
			}
		}
	}
}
