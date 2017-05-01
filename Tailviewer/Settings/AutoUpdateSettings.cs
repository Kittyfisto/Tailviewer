using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using log4net;
using Metrolib;

namespace Tailviewer.Settings
{
	public sealed class AutoUpdateSettings
		: ICloneable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private const int PasswordSaltLength = 20;

		public DateTime LastChecked;
		public bool CheckForUpdates;
		public bool AutomaticallyInstallUpdates;
		public string ProxyServer;
		public string ProxyUsername;

		public string ProxyPassword
		{
			get
			{
				try
				{
					if (_password == null || _password.Length == 0)
					{
						if (_passwordIsEmpty)
							return string.Empty;

						return null;
					}

					var unprotected = ProtectedData.Unprotect(_password, _passwordSalt, DataProtectionScope.CurrentUser);
					var value = Encoding.UTF8.GetString(unprotected);
					return value;
				}
				catch (Exception e)
				{
					Log.WarnFormat("Unable to retrieve password: {0}", e);
					return null;
				}
			}
			set
			{
				CreatePasswordSaltIfNecessary();

				try
				{
					if (!string.IsNullOrEmpty(value))
					{
						var data = Encoding.UTF8.GetBytes(value);
						var @protected = ProtectedData.Protect(data, _passwordSalt, DataProtectionScope.CurrentUser);
						_password = @protected;
					}
					else
					{
						_password = null;
						_passwordIsEmpty = value == string.Empty;
					}
				}
				catch (Exception e)
				{
					Log.WarnFormat("Unable to store password: {0}", e);
				}
			}
		}

		private byte[] _password;
		private byte[] _passwordSalt;
		private bool _passwordIsEmpty;

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeBool("checkforupdates", CheckForUpdates);
			writer.WriteAttributeBool("automaticallyinstallupdates", AutomaticallyInstallUpdates);
			writer.WriteAttributeBase64("passwordsalt", _passwordSalt);
			writer.WriteAttributeString("proxyserver", ProxyServer);
			writer.WriteAttributeString("proxyusername", ProxyUsername);
			writer.WriteAttributeBase64("proxypassword", _password);
		}

		public void Restore(XmlReader reader)
		{
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "checkforupdates":
						CheckForUpdates = reader.ReadContentAsBool();
						break;

					case "automaticallyinstallupdates":
						AutomaticallyInstallUpdates = reader.ReadContentAsBool();
						break;

					case "passwordsalt":
						_passwordSalt = reader.ReadContentAsBase64();
						break;

					case "proxyserver":
						ProxyServer = reader.ReadContentAsString();
						break;

					case "proxyusername":
						ProxyUsername = reader.ReadContentAsString();
						break;

					case "proxypassword":
						_password = reader.ReadContentAsBase64();
						break;

					default:
						Log.WarnFormat("Skipping unknown attribute '{0}'", reader.Name);
						break;
				}
			}

			CreatePasswordSaltIfNecessary();
		}

		private void CreatePasswordSaltIfNecessary()
		{
			if (_passwordSalt == null || _passwordSalt.Length != 20)
			{
				using (var rng = new RNGCryptoServiceProvider())
				{
					_passwordSalt = new byte[PasswordSaltLength];
					rng.GetBytes(_passwordSalt);
				}
			}
		}

		public ICredentials GetProxyCredentials()
		{
			var username = ProxyUsername;
			var password = ProxyPassword;

			if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
				return null;

			var securePassword = new SecureString();
			if (password != null)
			{
				foreach (var c in password)
				{
					securePassword.AppendChar(c);
				}
			}

			var credentials = new NetworkCredential(username, securePassword);
			return credentials;
		}

		public IWebProxy GetWebProxy()
		{
			var credentials = GetProxyCredentials();
			if (!string.IsNullOrEmpty(ProxyServer))
			{
				return new WebProxy(ProxyServer, true, null, credentials);
			}

			var proxy = WebRequest.GetSystemWebProxy();
			if (credentials != null)
			{
				proxy.Credentials = credentials;
			}
			return proxy;
		}

		[Pure]
		public AutoUpdateSettings Clone()
		{
			return new AutoUpdateSettings
			{
				AutomaticallyInstallUpdates = AutomaticallyInstallUpdates,
				CheckForUpdates = CheckForUpdates,
				LastChecked = LastChecked,
				ProxyServer = ProxyServer,
				ProxyUsername = ProxyUsername,
				_password = _password?.ToArray(),
				_passwordIsEmpty = _passwordIsEmpty,
				_passwordSalt = _passwordSalt?.ToArray()
			};
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}