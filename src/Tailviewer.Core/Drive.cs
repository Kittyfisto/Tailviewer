using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using log4net;

namespace Tailviewer.Core
{
	/// <summary>
	///     "Namespace" to collect methods related to drives.
	/// </summary>
	public static class Drive
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		///     Tests if a drive with the given letter exists AND is reachable.
		///     If <paramref name="driveLetter" /> represents a network drive, but the drive
		///     is disconnected/not reachable, then this method returns false.
		/// </summary>
		/// <param name="driveLetter"></param>
		/// <param name="timeout"></param>
		public static bool IsReachable(char driveLetter, TimeSpan timeout)
		{
			string name;
			var type = GetDriveType(driveLetter, out name);
			switch (type)
			{
				case null:
					return false;

				case DriveType.Network:
					var remoteName = new StringBuilder(capacity: 256);
					var length = remoteName.Capacity;
					var ret = WNetGetConnection(name, remoteName, ref length);
					if (ret != 0)
					{
						Log.ErrorFormat("Unable to get servername for networked drive '{0}', WNetGetConnection returned {1}", name, ret);
						return false;
					}
					else
					{
						var hostname = remoteName.ToString(startIndex: 0, length: length);
						return IsPingable(hostname, timeout);
					}

				default:
					return true;
			}
		}

		/// <summary>
		///     Pings the given host and returns true if a successful response was returned.
		/// </summary>
		/// <param name="hostname"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		private static bool IsPingable(string hostname, TimeSpan timeout)
		{
			try
			{
				var ping = new Ping();
				var reply = ping.Send(hostname, (int) timeout.TotalMilliseconds);
				if (reply == null)
					return false;

				var status = reply.Status;
				Log.DebugFormat("Pinged host '{0}', result: {1}", hostname, status);
				return status == IPStatus.Success;
			}
			catch (PingException e)
			{
				Log.InfoFormat("Failed to ping host '{0}': {1}", hostname, e.Message);
				return false;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while pinging host '{0}': {1}", hostname, e);
				return false;
			}
		}

		/// <summary>
		///     Tests if the given drive is a network drive.
		/// </summary>
		/// <param name="driveLetter"></param>
		/// <returns></returns>
		public static bool IsNetworkDrive(char driveLetter)
		{
			return GetDriveType(driveLetter) == DriveType.Network;
		}

		private static DriveType? GetDriveType(char driveLetter)
		{
			string unused;
			return GetDriveType(driveLetter, out unused);
		}

		private static DriveType? GetDriveType(char driveLetter, out string name)
		{
			var tmp = name = string.Format("{0}:\\", driveLetter);
			var drive = DriveInfo.GetDrives().FirstOrDefault(x => Equals(x.Name, tmp));
			return drive?.DriveType;
		}

		[DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern int WNetGetConnection(
			[MarshalAs(UnmanagedType.LPTStr)] string localName,
			[MarshalAs(UnmanagedType.LPTStr)] StringBuilder remoteName,
			ref int length);
	}
}