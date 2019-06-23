using System;
using System.IO;
using System.Reflection;

namespace Installer.Applications.Install
{
	public partial class LicenseWindow
	{
		private readonly string _licenseText;

		public LicenseWindow()
		{
			try
			{
				var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("InstallationFiles\\LICENSE");
				using (var reader = new StreamReader(stream))
				{
					_licenseText = reader.ReadToEnd();
				}
			}
			catch (Exception)
			{
				_licenseText =
					"Unable to display license text. Please go to https://github.com/Kittyfisto/Tailviewer/blob/master/LICENSE to view the license terms.";
			}

			InitializeComponent();
		}

		public string LicenseText
		{
			get { return _licenseText; }
		}
	}
}
