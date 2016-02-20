using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;
using QuickFilters = Tailviewer.BusinessLogic.Filters.QuickFilters;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class QuickFiltersTest
	{
		private Tailviewer.Settings.QuickFilters _settings;

		[SetUp]
		public void SetUp()
		{
			_settings = new Tailviewer.Settings.QuickFilters();
		}

		[Test]
		public void TestCtor()
		{
			_settings.Add(new QuickFilter{Value = "foo"});
			_settings.Add(new QuickFilter{Value = "bar"});
			var quickFilters = new QuickFilters(_settings);
			var filters = quickFilters.Filters.ToList();
			filters.Count.Should().Be(2);
			filters[0].Id.Should().Be(_settings[0].Id);
			filters[0].Value.Should().Be("foo");
			filters[1].Id.Should().Be(_settings[1].Id);
			filters[1].Value.Should().Be("bar");
		}
	}
}