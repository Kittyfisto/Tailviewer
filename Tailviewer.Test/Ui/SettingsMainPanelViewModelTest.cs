using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.MainPanel.Settings;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class SettingsMainPanelViewModelTest
	{
		private ApplicationSettings _settings;
		private SettingsMainPanelViewModel _model;

		[SetUp]
		public void SetUp()
		{
			_settings = new ApplicationSettings("foo");
			_model = new SettingsMainPanelViewModel(_settings);
		}

		[Test]
		public void TestProxyPassword1()
		{
			var changes = new List<string>();
			_model.PropertyChanged += (sender, args) => changes.Add(args.PropertyName);

			_model.ProxyPassword = "foobar";
			changes.Should().Equal(new object[] {"ProxyPassword"});

			_model.ProxyPassword = "blub";
			changes.Should().Equal(new object[]
				{
					"ProxyPassword",
					"ProxyPassword"
				});

			_model.ProxyPassword = "blub";
			changes.Should().Equal(new object[]
				{
					"ProxyPassword",
					"ProxyPassword"
				});
		}

		[Test]
		public void TestProxyPassword2()
		{
			_model.ProxyPassword = "foobar";
			_settings.AutoUpdate.ProxyPassword.Should().Be("foobar");

			_model.ProxyPassword = null;
			_settings.AutoUpdate.ProxyPassword.Should().BeNull();

			_model.ProxyPassword = string.Empty;
			_settings.AutoUpdate.ProxyPassword.Should().BeEmpty();
		}
	}
}