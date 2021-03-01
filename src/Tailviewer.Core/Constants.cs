using System;
using System.IO;
using System.Reflection;

namespace Tailviewer.Core
{
	/// <summary>
	///     Provides access to various constants pertaining to tailviewer (build, version, etc..)
	/// </summary>
	public static class Constants
	{
		/// <summary>
		///     The version of this tailviewer.
		/// </summary>
		public static readonly Version ApplicationVersion;

		/// <summary>
		/// The folder tailviewer is installed in.
		/// </summary>
		public static readonly string ApplicationFolder;

		/// <summary>
		///     The date this tailviewer was built.
		/// </summary>
		public static readonly DateTime BuildDate;

		static Constants()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var name = assembly.GetName();
			ApplicationVersion = name.Version;
			ApplicationFolder = assembly.GetFolder();
			BuildDate = GetLinkerTime(assembly);
		}

		/// <summary>
		///     We obtain the build date from the PE header of this assembly,
		///     http://stackoverflow.com/questions/1600962/displaying-the-build-date.
		/// </summary>
		/// <param name="assembly"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		private static DateTime GetLinkerTime(Assembly assembly, TimeZoneInfo target = null)
		{
			var filePath = assembly.Location;
			const int c_PeHeaderOffset = 60;
			const int c_LinkerTimestampOffset = 8;

			var buffer = new byte[2048];

			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				stream.Read(buffer, offset: 0, count: 2048);
			}

			var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
			var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
			var epoch = new DateTime(year: 1970, month: 1, day: 1, hour: 0, minute: 0, second: 0, kind: DateTimeKind.Utc);

			var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

			var tz = target ?? TimeZoneInfo.Local;
			var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

			return localTime;
		}
	}
}