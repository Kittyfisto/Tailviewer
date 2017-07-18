using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Settings;
using Tailviewer.Settings;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class QuickFiltersTest
	{
		[SetUp]
		public void SetUp()
		{
			_settings = new QuickFilters();
		}

		private QuickFilters _settings;

		[Test]
		public void TestCtor()
		{
			_settings.Add(new QuickFilter {Value = "foo"});
			_settings.Add(new QuickFilter {Value = "bar"});
			var quickFilters = new Tailviewer.BusinessLogic.Filters.QuickFilters(_settings);
			List<Tailviewer.BusinessLogic.Filters.QuickFilter> filters = quickFilters.Filters.ToList();
			filters.Count.Should().Be(2);
			filters[0].Id.Should().Be(_settings[0].Id);
			filters[0].Value.Should().Be("foo");
			filters[1].Id.Should().Be(_settings[1].Id);
			filters[1].Value.Should().Be("bar");
		}
	}
}