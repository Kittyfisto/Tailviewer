using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Settings;
using Tailviewer.QuickInfo.BusinessLogic;
using Tailviewer.QuickInfo.Ui;

namespace Tailviewer.QuickInfo.Test.Ui
{
	[TestFixture]
	public sealed class QuickInfoViewModelTest
	{
		[SetUp]
		public void Setup()
		{
			_id = Guid.NewGuid();
			_viewConfig = new QuickInfoViewConfiguration();
			_analyserConfig = new QuickInfoConfiguration();
		}

		private Guid _id;
		private QuickInfoViewConfiguration _viewConfig;
		private QuickInfoConfiguration _analyserConfig;

		[Test]
		public void TestChangeFormat()
		{
			_analyserConfig.MatchType = FilterMatchType.SubstringFilter;
			var model = new QuickInfoViewModel(_id, _viewConfig, _analyserConfig);
			model.Result = new QuickInfo.BusinessLogic.QuickInfo("Foobar");
			model.Format = "Match: {message} Found!";
			model.Value.Should().Be("Match: Foobar Found!");

			model.Format = "{timestamp}";
			model.Value.Should().Be("N/A", "because the match doesn't include a timestamp");
		}

		[Test]
		public void TestCtor()
		{
			var model = new QuickInfoViewModel(_id, _viewConfig, _analyserConfig);
			model.Result.Should().BeNull();
			model.Value.Should().BeNull();
		}

		[Test]
		public void TestFormat1()
		{
			_analyserConfig.MatchType = FilterMatchType.SubstringFilter;
			var model = new QuickInfoViewModel(_id, _viewConfig, _analyserConfig);
			model.Result = new QuickInfo.BusinessLogic.QuickInfo("Foobar");
			model.Value.Should().Be("Foobar", "because by default, the entire matched line shall be printed");
		}

		[Test]
		public void TestSetNullValue()
		{
			_analyserConfig.MatchType = FilterMatchType.SubstringFilter;
			var model = new QuickInfoViewModel(_id, _viewConfig, _analyserConfig);
			model.Result = new QuickInfo.BusinessLogic.QuickInfo("Foobar");
			model.Result = null;
			model.Value.Should().Be("N/A", "because we haven't forwarded a result just yet");
		}
	}
}