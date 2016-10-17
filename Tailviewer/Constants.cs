using System;
using System.IO;
using System.Reflection;

namespace Tailviewer
{
	public static class Constants
	{
		public static readonly string ApplicationTitle;
		public static readonly string MainWindowTitle;
		public static readonly Version ApplicationVersion;
		public static readonly Uri ProjectPage;
		public static readonly Uri GithubPage;
		public static readonly string ApplicationFolder;
		public static readonly string AppDataLocalFolder;
		public static readonly string DownloadFolder;
		public static string ApplicationLicense
		{
			get
			{
				return Resource.ReadResourceToEnd("Licenses/Tailviewer/LICENSE");
			}
		}

		static Constants()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var name = assembly.GetName();

			ApplicationTitle = "Tailviewer";
			ApplicationVersion = name.Version;
			MainWindowTitle = string.Format("Tailviewer, v{0}", ApplicationVersion.Format());
			ProjectPage = new Uri("https://kittyfisto.github.io/Tailviewer/");
			GithubPage = new Uri("https://github.com/Kittyfisto/Tailviewer");
			ApplicationFolder = assembly.GetFolder();
			AppDataLocalFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationTitle);
			DownloadFolder = Path.Combine(AppDataLocalFolder, "Downloads");
		}
	}
}