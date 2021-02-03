using System.Net;

namespace Tailviewer.DataSources.UDP
{
	public struct UdpDatagram
	{
		public IPEndPoint Sender;
		public byte[] Message;
	}
}