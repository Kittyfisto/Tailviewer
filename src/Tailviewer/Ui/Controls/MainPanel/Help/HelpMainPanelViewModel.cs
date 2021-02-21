using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.SidePanel;

namespace Tailviewer.Ui.Controls.MainPanel.Help
{
	class HelpMainPanelViewModel
		: AbstractMainPanelViewModel
	{
		public HelpMainPanelViewModel(IApplicationSettings applicationSettings) : base(applicationSettings)
		{
		}

		public Uri Source => new Uri(@"C:\Users\Simon\Documents\GitHub\Tailviewer\help\index.html");

		public static string DefaultWebBrowser
		{
			get
			{
				string path = @"\http\shell\open\command";

				using (RegistryKey reg = Registry.ClassesRoot.OpenSubKey(path))
				{
					if (reg != null)
					{
						string webBrowserPath = reg.GetValue(String.Empty) as string;

						if (!String.IsNullOrEmpty(webBrowserPath))
						{
							if (webBrowserPath.First() == '"')
							{
								return webBrowserPath.Split('"')[1];
							}

							return webBrowserPath.Split(' ')[0];
						}
					}

					return null;
				}
			}
		}

		#region Overrides of AbstractMainPanelViewModel

		public override IEnumerable<ISidePanelViewModel> SidePanels => Enumerable.Empty<ISidePanelViewModel>();

		public override void Update()
		{}

		#endregion
	}
}
