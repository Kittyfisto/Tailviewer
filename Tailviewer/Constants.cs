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
		public static readonly DateTime BuildDate;
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

		/// <summary>
		/// We obtain the build date from the PE header of this assembly, <see cref="http://stackoverflow.com/questions/1600962/displaying-the-build-date"/>.
		/// </summary>
		/// <param name="assembly"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static DateTime GetLinkerTime(this Assembly assembly, TimeZoneInfo target = null)
		{
			var filePath = assembly.Location;
			const int c_PeHeaderOffset = 60;
			const int c_LinkerTimestampOffset = 8;

			var buffer = new byte[2048];

			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				stream.Read(buffer, 0, 2048);

			var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
			var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

			var tz = target ?? TimeZoneInfo.Local;
			var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

			return localTime;
		}
	}
}