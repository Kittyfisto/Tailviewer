using System;
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
		public static string ApplicationLicense
		{
			get
			{
				return Resource.ReadResourceToEnd("Licenses/Tailviewer/LICENSE");
			}
		}

		static Constants()
		{
			ApplicationTitle = "Tailviewer";
			ApplicationVersion = Assembly.GetExecutingAssembly().GetName().Version;
			MainWindowTitle = string.Format("Tailviewer, v{0}", ApplicationVersion);
			ProjectPage = new Uri("https://kittyfisto.github.io/Tailviewer/");
			GithubPage = new Uri("https://github.com/Kittyfisto/Tailviewer");
		}
	}
}