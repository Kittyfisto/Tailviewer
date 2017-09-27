using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo;
using Tailviewer.Core.Settings;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.QuickInfo;

namespace Tailviewer.Test.Ui.Controls.Analyse.Widgets.QuickInfo
{
	[TestFixture]
	public sealed class QuickInfoViewModelTest
	{
		private Guid _id;
		private QuickInfoViewConfiguration _viewConfig;
		private QuickInfoConfiguration _analyserConfig;

		[SetUp]
		public void Setup()
		{
			_id = Guid.NewGuid();
			_viewConfig = new QuickInfoViewConfiguration();
			_analyserConfig = new QuickInfoConfiguration();
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
			_analyserConfig.MatchType = FilterMatchType.StringFilter;
			var model = new QuickInfoViewModel(_id, _viewConfig, _analyserConfig);
			model.Result = new Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo.QuickInfo("Foobar");
			model.Value.Should().Be("Foobar", "because by default, the entire matched line shall be printed");
		}

		[Test]
		public void TestChangeFormat()
		{
			_analyserConfig.MatchType = FilterMatchType.StringFilter;
			var model = new QuickInfoViewModel(_id, _viewConfig, _analyserConfig);
			model.Result = new Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo.QuickInfo("Foobar");
			model.Format = "Match: {message} Found!";
			model.Value.Should().Be("Match: Foobar Found!");

			model.Format = "{timestamp}";
			model.Value.Should().Be("N/A", "because the match doesn't include a timestamp");
		}
	}
}
