using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Settings;
using Tailviewer.QuickInfo.BusinessLogic;
using Tailviewer.QuickInfo.Ui;
using Tailviewer.Test;

namespace Tailviewer.QuickInfo.Test.BusinessLogic
{
	[TestFixture]
	public sealed class QuickInfoConfigurationTest
	{
		[Test]
		public void TestConstruction()
		{
			var config = new QuickInfoConfiguration();
			config.FilterValue.Should().BeNull();
			config.MatchType.Should().Be(FilterMatchType.RegexpFilter);
		}
	}
}