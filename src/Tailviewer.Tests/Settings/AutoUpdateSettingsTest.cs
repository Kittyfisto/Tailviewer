using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;

namespace Tailviewer.Tests.Settings
{
	[TestFixture]
	public sealed class AutoUpdateSettingsTest
	{
		private AutoUpdateSettings _settings;

		public static IEnumerable<IReadOnlyList<string>> PluginRepositories =>
			new IReadOnlyList<string>[]
			{
				new string[0],
				new[] {"tvpr://blabla.com"}
			};

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

		[Test]
		public void TestGetWebProxy1()
		{
			_settings.ProxyServer = null;
			_settings.ProxyUsername = null;
			_settings.ProxyPassword = null;
			new Action(() => _settings.GetWebProxy()).Should().NotThrow();
			var proxy = _settings.GetWebProxy();
			proxy.Should().NotBeNull();
			proxy.Credentials.Should().BeNull();
		}

		[Test]
		public void TestGetWebProxy2()
		{
			_settings.ProxyServer = "http://eumex.ip";
			var proxy = _settings.GetWebProxy();
			proxy.Should().NotBeNull();
			proxy.Should().BeOfType<WebProxy>();
			((WebProxy)proxy).Address.Should().Be(new Uri("http://eumex.ip"));
			proxy.Credentials.Should().BeNull();
		}

		[Test]
		public void TestGetWebProxy3()
		{
			_settings.ProxyUsername = "foo";
			_settings.ProxyPassword = "bar";
			var proxy = _settings.GetWebProxy();
			var credentials = proxy.Credentials;
			credentials.Should().NotBeNull();
			credentials.Should().BeOfType<NetworkCredential>();
			((NetworkCredential) credentials).UserName.Should().Be("foo");
			((NetworkCredential) credentials).SecurePassword.Length.Should().Be(3);
			((NetworkCredential) credentials).Password.Should().Be("bar");
		}

		[Test]
		public void TestGetWebProxy4()
		{
			_settings.ProxyServer = "http://eumex.ip";
			_settings.ProxyUsername = "foo";
			_settings.ProxyPassword = "bar";
			var proxy = _settings.GetWebProxy();
			var credentials = proxy.Credentials;
			credentials.Should().NotBeNull();
			proxy.Should().BeOfType<WebProxy>();
			((WebProxy)proxy).Address.Should().Be(new Uri("http://eumex.ip"));
		}

		[Test]
		public void TestStoreRestore([ValueSource(nameof(PluginRepositories))] IReadOnlyList<string> repositories)
		{
			_settings.ProxyServer = "http://eumex.ip";
			_settings.ProxyUsername = "foo";
			_settings.ProxyPassword = "bar";
			_settings.AutomaticallyInstallUpdates = true;
			_settings.CheckForUpdates = true;
			_settings.PluginRepositories = repositories;

			using (var stream = new MemoryStream())
			{
				using (var writer = XmlWriter.Create(stream))
				{
					writer.WriteStartElement("xml");
					writer.WriteStartElement("autoupdate");
					_settings.Save(writer);
					writer.WriteEndElement();
					writer.WriteEndElement();
				}

				stream.Position = 0;

				using (var reader = XmlReader.Create(stream))
				{
					var settings = new AutoUpdateSettings();
					reader.Read();
					reader.Read();
					reader.Read();
					settings.Restore(reader);

					settings.CheckForUpdates.Should().BeTrue();
					settings.AutomaticallyInstallUpdates.Should().BeTrue();
					settings.ProxyServer.Should().Be("http://eumex.ip");
					settings.ProxyUsername.Should().Be("foo");
					settings.ProxyPassword.Should().Be("bar");
					settings.PluginRepositories.Should().Equal(repositories);
				}
			}
		}

		[Test]
		public void TestClone()
		{
			var settings = new AutoUpdateSettings
			{
				AutomaticallyInstallUpdates = true,
				CheckForUpdates = true,
				LastChecked = new DateTime(2017, 5, 1, 17, 30, 0),
				ProxyUsername = "user",
				ProxyPassword = "password",
				ProxyServer = "myproxy",
				PluginRepositories = new []{"tvpr://foobar:1241"}
			};
			var clone = settings.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(settings);
			clone.LastChecked.Should().Be(new DateTime(2017, 5, 1, 17, 30, 0));
			clone.ProxyUsername.Should().Be("user");
			clone.ProxyPassword.Should().Be("password");
			clone.ProxyServer.Should().Be("myproxy");
			clone.PluginRepositories.Should().Equal(new object[]
			{
				"tvpr://foobar:1241"
			});
		}
	}
}