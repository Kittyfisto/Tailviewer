using FluentAssertions;
using NUnit.Framework;
using Tailviewer.QuickInfo.Ui;

namespace Tailviewer.QuickInfo.Test.Ui
{
	[TestFixture]
	public sealed class QuickInfoViewConfigurationTest
	{
		[Test]
		public void TestCtor()
		{
			var config = new QuickInfoViewConfiguration();
			config.Name.Should().Be("New Quick Info");
			config.Format.Should().Be("{message}");
		}
	}
}