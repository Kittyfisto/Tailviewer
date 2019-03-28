using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Analysis.DataSources.Ui;
using Tailviewer.Core;

namespace Tailviewer.Analysis.DataSources.Test.Ui
{
	[TestFixture]
	public sealed class DataSourcesWidgetConfigurationTest
	{
		[Test]
		public void TestConstruction()
		{
			var config = new DataSourcesWidgetConfiguration();
			config.ShowFileName.Should().BeTrue();
			config.ShowFileSize.Should().BeFalse();
			config.ShowCreated.Should().BeFalse();
			config.ShowLastModified.Should().BeFalse();
		}

		[Test]
		public void TestRoundtrip([Values(true, false)] bool showFileName,
			                      [Values(true, false)] bool showFileSize,
			                      [Values(true, false)] bool showCreated,
			                      [Values(true, false)] bool showLastModified)
		{
			var config = new DataSourcesWidgetConfiguration
			{
				ShowFileName = showFileName,
				ShowFileSize = showFileSize,
				ShowCreated = showCreated,
				ShowLastModified = showLastModified
			};
			var actualConfig = config.Roundtrip();
			actualConfig.ShowFileName.Should().Be(showFileName);
			actualConfig.ShowFileSize.Should().Be(showFileSize);
			actualConfig.ShowCreated.Should().Be(showCreated);
			actualConfig.ShowLastModified.Should().Be(showLastModified);
		}

		[Test]
		public void TestClone([Values(true, false)] bool showFileName,
			                  [Values(true, false)] bool showFileSize,
			                  [Values(true, false)] bool showCreated,
			                  [Values(true, false)] bool showLastModified)
		{
			var config = new DataSourcesWidgetConfiguration
			{
				ShowFileName = showFileName,
				ShowFileSize = showFileSize,
				ShowCreated = showCreated,
				ShowLastModified = showLastModified
			};
			var actualConfig = config.Clone();
			actualConfig.ShowFileName.Should().Be(showFileName);
			actualConfig.ShowFileSize.Should().Be(showFileSize);
			actualConfig.ShowCreated.Should().Be(showCreated);
			actualConfig.ShowLastModified.Should().Be(showLastModified);
		}
	}
}
