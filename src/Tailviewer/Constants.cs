using System;
using System.IO;
using Metrolib;

namespace Tailviewer
{
	public static class Constants
	{
		public static readonly string ApplicationTitle;

		private const string ApplicationFolderName = "Tailviewer NEXT";
		public static readonly string MainWindowTitle;
		public static readonly Version ApplicationVersion;
		public static readonly DateTime BuildDate;
		public static readonly Uri ProjectPage;
		public static readonly Uri GithubPage;
		public static readonly Uri ReportBugPage;
		public static readonly string ApplicationFolder;
		public static readonly string AppDataLocalFolder;
		public static readonly string ApplicationLogFile;
		public static readonly string MyDocumentsFolder;
		public static readonly string ExportDirectory;
		public static readonly string DownloadFolder;
		public static readonly string PluginPath;

		/// <summary>
		///    Plugins which have been automatically updated are
		///    stored in this folder.
		/// </summary>
		public static readonly string DownloadedPluginsPath;

		public static readonly string SnapshotDirectory;
		public static readonly string SnapshotExtension;

		public static readonly string AnalysisDirectory;
		public static readonly string AnalysisExtension;

		public static readonly string AnalysisTemplateDirectory;
		public static readonly string AnalysisTemplateExtension;

		public static string ApplicationLicense => Resource.ReadResourceToEnd("Licenses/Tailviewer/LICENSE");

		static Constants()
		{
			ApplicationTitle = "Tailviewer NEXT";
			ApplicationVersion = Core.Constants.ApplicationVersion;
			BuildDate = Core.Constants.BuildDate;
			MainWindowTitle = string.Format("Tailviewer NEXT, v{0}", ApplicationVersion.Format());
			ProjectPage = new Uri("https://github.com/brofea/Tailviewer-NEXT");
			GithubPage = new Uri("https://github.com/brofea/Tailviewer-NEXT");
			ReportBugPage = new Uri("https://github.com/brofea/Tailviewer-NEXT/issues/new");
			ApplicationFolder = Core.Constants.ApplicationFolder;
			PluginPath = Path.Combine(ApplicationFolder, "Plugins");
			AppDataLocalFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationFolderName);

			#if DEBUG
			ApplicationLogFile = Path.Combine(ApplicationFolder, "Tailviewer.log");
			#else
			ApplicationLogFile = Path.Combine(AppDataLocalFolder, "Tailviewer.log");
			#endif

			DownloadFolder = Path.Combine(AppDataLocalFolder, "Downloads");
			DownloadedPluginsPath = Path.Combine(DownloadFolder, "Plugins");

			MyDocumentsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ApplicationFolderName);
			ExportDirectory = Path.Combine(MyDocumentsFolder, "Export");

			SnapshotDirectory = Path.Combine(MyDocumentsFolder, "Snapshots");
			SnapshotExtension = "tvas";

			AnalysisDirectory = Path.Combine(MyDocumentsFolder, "Analyses");
			AnalysisExtension = "tva";

			AnalysisTemplateDirectory = Path.Combine(MyDocumentsFolder, "Templates");
			AnalysisTemplateExtension = "tvat";
		}
	}
}