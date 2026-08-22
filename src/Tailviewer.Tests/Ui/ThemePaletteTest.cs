using System.Windows.Media;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Ui;

namespace Tailviewer.Tests.Ui
{
	[TestFixture]
	public sealed class ThemePaletteTest
	{
		[Test]
		public void TestComputePrimary()
		{
			var primary = Color.FromRgb(0x00, 0x47, 0xAB);
			var palette = ThemePalette.Compute(primary);

			palette.Primary.Should().Be(primary);
			palette.Secondary.Should().Be(primary);
		}

		[Test]
		public void TestFixedForegrounds()
		{
			var palette = ThemePalette.Compute(Color.FromRgb(0x00, 0x47, 0xAB));

			palette.PrimaryForeground.Should().Be(Colors.White);
			palette.SecondaryForeground.Should().Be(Colors.Black);
		}

		[Test]
		public void TestDerivedShadesAreDeterministic()
		{
			var primary = Color.FromRgb(0x00, 0x47, 0xAB);
			var a = ThemePalette.Compute(primary);
			var b = ThemePalette.Compute(primary);

			a.PrimaryLight.Should().Be(b.PrimaryLight);
			a.PrimaryLighter.Should().Be(b.PrimaryLighter);
			a.PrimaryDark.Should().Be(b.PrimaryDark);
			a.PrimarySeparator.Should().Be(a.PrimaryDark);
		}

		[Test]
		public void TestShadesAreLighterAndDarker()
		{
			var palette = ThemePalette.Compute(Color.FromRgb(0x00, 0x47, 0xAB));

			((int) palette.PrimaryLighter.R).Should().BeGreaterThan((int) palette.Primary.R);
			((int) palette.PrimaryDark.B).Should().BeLessThan((int) palette.Primary.B);
		}
	}
}
