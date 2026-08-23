using System.Windows.Media;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Ui;

namespace Tailviewer.Tests.Ui
{
	[TestFixture]
	public sealed class SemanticPaletteTest
	{
		[Test]
		public void TestLightPalette()
		{
			var palette = SemanticPalette.Compute(false);

			palette.Surface.Should().Be(Color.FromRgb(0xFF, 0xFF, 0xFF));
			palette.SurfaceMuted.Should().Be(Color.FromRgb(0xD8, 0xD8, 0xD8));
			palette.TextPrimary.Should().Be(Color.FromRgb(0x33, 0x33, 0x33));
			palette.TextSecondary.Should().Be(Color.FromRgb(0xA0, 0xA0, 0xA0));
			palette.Divider.Should().Be(Color.FromRgb(0x80, 0x80, 0x80));
			palette.TitleBar.Should().Be(Color.FromRgb(0xEA, 0xED, 0xF2));
			palette.OverlayBackground.Should().Be(Color.FromArgb(0x55, 0x00, 0x00, 0x00));
			palette.DisabledForeground.Should().Be(Color.FromRgb(0xB7, 0xB7, 0xB7));
		}

		[Test]
		public void TestDarkPalette()
		{
			var palette = SemanticPalette.Compute(true);

			palette.Surface.Should().Be(Color.FromRgb(0x1E, 0x1E, 0x1E));
			palette.SurfaceMuted.Should().Be(Color.FromRgb(0x2D, 0x2D, 0x30));
			palette.TextPrimary.Should().Be(Color.FromRgb(0xDC, 0xDC, 0xDC));
			palette.TextSecondary.Should().Be(Color.FromRgb(0x9E, 0x9E, 0x9E));
			palette.Divider.Should().Be(Color.FromRgb(0x3F, 0x3F, 0x46));
			palette.TitleBar.Should().Be(Color.FromRgb(0x25, 0x25, 0x26));
			palette.OverlayBackground.Should().Be(Color.FromArgb(0x99, 0x00, 0x00, 0x00));
			palette.DisabledForeground.Should().Be(Color.FromRgb(0x6B, 0x6B, 0x6B));
		}

		[Test]
		public void TestUnfocusedOverlayIsStable()
		{
			SemanticPalette.Compute(false).UnfocusedOverlay.Should().Be(SemanticPalette.Compute(true).UnfocusedOverlay);
		}
	}
}
