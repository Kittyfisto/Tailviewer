using System;

namespace Tailviewer.DataSources.UDP
{
	public interface IUdpSocket
		: IDisposable
	{
		event Action<UdpDatagram> OnMessage;
	}
}