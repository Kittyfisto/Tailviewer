using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;
using Tailviewer.Ui.LogView;

namespace Tailviewer.Test.Ui.Controls.LogView
{
	[TestFixture]
	[RequiresThread(ApartmentState.STA)]
	public sealed class DataSourceDisplayModeToggleButtonTest
	{
		[Test]
		public void TestCtor()
		{
			var toggle = new DataSourceDisplayModeToggleButton();
			toggle.DisplayMode.Should().Be(DataSourceDisplayMode.Filename);
			toggle.ShowFilename.Should().BeTrue();
			toggle.ShowCharacterCode.Should().BeFalse();
		}

		[Test]
		public void TestChangeDisplayMode()
		{
			var toggle = new DataSourceDisplayModeToggleButton();

			toggle.DisplayMode = DataSourceDisplayMode.CharacterCode;
			toggle.ShowFilename.Should().BeFalse();
			toggle.ShowCharacterCode.Should().BeTrue();

			toggle.DisplayMode = DataSourceDisplayMode.Filename;
			toggle.ShowFilename.Should().BeTrue();
			toggle.ShowCharacterCode.Should().BeFalse();
		}

		[Test]
		public void TestChangeShowCharacterCode()
		{
			var toggle = new DataSourceDisplayModeToggleButton();

			toggle.ShowCharacterCode = true;
			toggle.ShowCharacterCode.Should().BeTrue();
			toggle.ShowFilename.Should().BeFalse();
			toggle.DisplayMode.Should().Be(DataSourceDisplayMode.CharacterCode);
		}

		[Test]
		public void TestChangeShowFilename()
		{
			var toggle = new DataSourceDisplayModeToggleButton {DisplayMode = DataSourceDisplayMode.CharacterCode};

			toggle.ShowFilename = true;
			toggle.ShowFilename.Should().BeTrue();
			toggle.ShowCharacterCode.Should().BeFalse();
			toggle.DisplayMode.Should().Be(DataSourceDisplayMode.Filename);
		}
	}
}