using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Ui.Controls.LogView;

namespace Tailviewer.Test.Ui.Controls.LogView
{
	[TestFixture]
	public sealed class AdjustingDoubleConverterTest
	{
		[Test]
		public void TestConvert1([Values(1.0, 2.0, 0.5, -42.0)] double value)
		{
			var converter = new AdjustingDoubleConverter();
			converter.Convert(value, typeof(double), null, CultureInfo.CurrentCulture)
				.Should().Be(value);
		}

		[Test]
		public void TestConvert2([Values(double.NaN, double.PositiveInfinity, double.NegativeInfinity)] double invalidValue)
		{
			var converter = new AdjustingDoubleConverter();
			converter.Convert(invalidValue, typeof(double), null, CultureInfo.CurrentCulture)
				.Should().Be(0.0);
		}

		[Test]
		[Description("Verifies that the converter tolerates invalid input data and silently ignores it")]
		public void TestConvert3()
		{
			var converter = new AdjustingDoubleConverter();
			new Action(() => converter.Convert(null, typeof(double), null, CultureInfo.CurrentCulture)).ShouldNotThrow();
			new Action(() => converter.Convert(42, typeof(double), null, CultureInfo.CurrentCulture)).ShouldNotThrow();
			new Action(() => converter.Convert(42f, typeof(double), null, CultureInfo.CurrentCulture)).ShouldNotThrow();
			new Action(() => converter.Convert("42", typeof(double), null, CultureInfo.CurrentCulture)).ShouldNotThrow();
			new Action(() => converter.Convert(typeof(bool), typeof(double), null, CultureInfo.CurrentCulture)).ShouldNotThrow();
		}
	}
}
