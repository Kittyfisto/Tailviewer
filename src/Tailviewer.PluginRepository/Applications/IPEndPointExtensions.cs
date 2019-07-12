using System.Net;

namespace Tailviewer.PluginRepository.Applications
{
	public static class IPEndPointExtensions
	{
		public static IPEndPoint Parse(string address)
		{
			var idx = address.LastIndexOf(':');
			var ipAddress = IPAddress.Parse(address.Substring(0, idx));
			var port = int.Parse(address.Substring(idx+1));
			return new IPEndPoint(ipAddress, port);
		}
	}
}