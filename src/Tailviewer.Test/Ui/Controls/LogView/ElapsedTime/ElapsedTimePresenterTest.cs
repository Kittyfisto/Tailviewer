using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Ui.Controls.LogView.ElapsedTime;

namespace Tailviewer.Test.Ui.Controls.LogView.ElapsedTime
{
	[TestFixture]
	public sealed class ElapsedTimePresenterTest
	{
		[Test]
		public void TestToString1()
		{
			var culture = CultureInfo.InvariantCulture;
			ElapsedTimeFormatter.ToString(TimeSpan.FromMilliseconds(1), culture)
			                    .Should().Be("00:00:00.001");
		}

		[Test]
		public void TestToString2()
		{
			var culture = CultureInfo.InvariantCulture;
			ElapsedTimeFormatter.ToString(TimeSpan.FromSeconds(1), culture)
			                    .Should().Be("00:00:01.000");
		}

		[Test]
		public void TestToString3()
		{
			var culture = CultureInfo.InvariantCulture;
			ElapsedTimeFormatter.ToString(TimeSpan.FromMinutes(1), culture)
			                    .Should().Be("00:01:00.000");
		}

		[Test]
		public void TestToString4()
		{
			var culture = CultureInfo.InvariantCulture;
			ElapsedTimeFormatter.ToString(TimeSpan.FromDays(1), culture)
			                    .Should().Be("1d 00:00:00.000");
		}
	}
}