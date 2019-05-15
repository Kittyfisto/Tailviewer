using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Analysis.QuickInfo.Ui;
using Tailviewer.Core;

namespace Tailviewer.Analysis.QuickInfo.Test.Ui
{
	[TestFixture]
	public sealed class QuickInfoWidgetConfigurationTest
	{
		[Test]
		public void TestConstruction()
		{
			var config = new QuickInfoViewConfiguration();
			config.Name.Should().Be("New Quick Info");
			config.Format.Should().Be("{message}");
		}

		[Test]
		public void TestRoundtripEmpty()
		{
			var config = new QuickInfoWidgetConfiguration();
			var actualConfig = config.Roundtrip();
			actualConfig.Should().NotBeNull();
			actualConfig.Titles.Should().BeEmpty();
		}

		[Test]
		public void TestRoundtripOneTitle()
		{
			var config = new QuickInfoWidgetConfiguration();
			var id = Guid.NewGuid();
			config.Add(new QuickInfoViewConfiguration(id){Name = "Version", Format = "Stuff {message}"});

			var actualConfig = config.Roundtrip(typeof(QuickInfoViewConfiguration));
			actualConfig.Should().NotBeNull();
			actualConfig.Titles.Should().HaveCount(1);
			actualConfig.Titles.First().Id.Should().Be(id);
			actualConfig.Titles.First().Name.Should().Be("Version");
			actualConfig.Titles.First().Format.Should().Be("Stuff {message}");
		}
	}
}