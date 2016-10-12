using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;

namespace Tailviewer.Test.Settings
{
	[TestFixture]
	public sealed class AutoUpdateSettingsTest
	{
		private AutoUpdateSettings _settings;

		[SetUp]
		public void Setup()
		{
			_settings = new AutoUpdateSettings();
		}

		[Test]
		public void TestPassword()
		{
			_settings.ProxyPassword = "password";
			_settings.ProxyPassword.Should().Be("password");

			_settings.ProxyPassword = null;
			_settings.ProxyPassword.Should().BeNull();

			_settings.ProxyPassword = string.Empty;
			_settings.ProxyPassword.Should().BeEmpty();
		}

		[Test]
		public void TestGetProxyCredentials()
		{
			_settings.GetProxyCredentials().Should().BeNull();

			_settings.ProxyUsername = "foo";
			_settings.GetProxyCredentials().Should().NotBeNull();

			_settings.ProxyUsername = null;
			_settings.ProxyPassword = "bar";
			_settings.GetProxyCredentials().Should().NotBeNull();

			_settings.ProxyUsername = "foo";
			_settings.ProxyPassword = "bar";
			_settings.GetProxyCredentials().Should().NotBeNull();
		}
	}
}