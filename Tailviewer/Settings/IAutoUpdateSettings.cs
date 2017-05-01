using System;
using System.Net;
using System.Xml;

namespace Tailviewer.Settings
{
	public interface IAutoUpdateSettings
		
	{
		DateTime LastChecked { get; set; }
		bool CheckForUpdates { get; set; }
		bool AutomaticallyInstallUpdates { get; set; }
		string ProxyServer { get; set; }
		string ProxyUsername { get; set; }
		string ProxyPassword { get; set; }

		void Save(XmlWriter writer);
		void Restore(XmlReader reader);
		ICredentials GetProxyCredentials();
		IWebProxy GetWebProxy();
	}
}