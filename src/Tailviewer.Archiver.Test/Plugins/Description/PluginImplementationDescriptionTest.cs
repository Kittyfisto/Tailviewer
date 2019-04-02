using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Archiver.Test.Plugins.Description
{
	[TestFixture]
	public sealed class PluginImplementationDescriptionTest
	{
		[PluginInterfaceVersion(42)]
		interface ISomeCoolPlugin
			: IPlugin
		{}

		[Test]
		public void TestConstruction()
		{
			var description = new PluginImplementationDescription("A.B.SomeType", typeof(ISomeCoolPlugin));
			description.FullTypeName.Should().Be("A.B.SomeType");
			description.Version.Should().Be(new PluginInterfaceVersion(42));
		}
	}
}
