using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Ui.Converters;

namespace Tailviewer.Test.Ui.Converters
{
	[TestFixture]
	public sealed class CountConverterTest
	{
		[SetUp]
		public void SetUp()
		{
			_converter = new CountConverter();
		}

		private CountConverter _converter;

		[Test]
		public void TestConvertWithSuffix()
		{
			_converter.Suffix = "Info";
			_converter.Convert(1024, typeof (string), null, CultureInfo.CurrentUICulture)
			          .Should().Be("1k Infos");

			_converter.Convert(1, typeof (string), null, CultureInfo.CurrentUICulture)
			          .Should().Be("1 Info");

			_converter.Convert(0, typeof (string), null, CultureInfo.CurrentUICulture)
			          .Should().Be("0 Infos");
		}

		[Test]
		public void TestConvertWithoutSuffix()
		{
			_converter.Convert(1024, typeof (string), null, CultureInfo.CurrentUICulture)
			          .Should().Be("1k");
		}
	}
}