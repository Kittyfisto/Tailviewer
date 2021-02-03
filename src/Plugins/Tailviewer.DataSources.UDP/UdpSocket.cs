using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using log4net;

namespace Tailviewer.DataSources.UDP
{
	public sealed class UdpSocket
		: IUdpSocket
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IPEndPoint _localEndPoint;
		private readonly byte[] _buffer;
		private EndPoint _remoteEndPoint;
		private readonly Socket _socket;
		private bool _isDisposed;

		public UdpSocket(IPEndPoint ipEndPoint)
		{
			_localEndPoint = ipEndPoint;
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			_socket.SetSocketOption(SocketOptionLevel.Socket,
			                        SocketOptionName.ReuseAddress, optionValue: true);
			_socket.Bind(ipEndPoint);

			const int bufferSize = 1024;
			_buffer = new byte[bufferSize];

			BeginReceive();
		}

		private void BeginReceive()
		{
			lock (_socket)
			{
				if (_isDisposed)
					return;

				EndPoint endPoint = _localEndPoint;
				_socket.BeginReceiveFrom(_buffer, offset: 0, size: _buffer.Length,
				                         socketFlags: SocketFlags.None,
				                         remoteEP: ref endPoint,
				                         callback: OnReceived,
				                         state: null);
			}
		}

		private void OnReceived(IAsyncResult ar)
		{
			try
			{
				EndPoint remoteEndPoint = _localEndPoint;
				int bytesReceived;

				lock (_socket)
				{
					if (_isDisposed)
						return;

					bytesReceived = _socket.EndReceiveFrom(ar, ref remoteEndPoint);
				}

				EmitOnMessage(bytesReceived, remoteEndPoint as IPEndPoint);
			}
			catch (SocketException e)
			{
				Log.ErrorFormat("Caught socket error: {0}", e);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
			finally
			{
				BeginReceive();
			}
		}

		private void EmitOnMessage(int bytesReceived, IPEndPoint sender)
		{
			try
			{
				var fn = OnMessage;
				if (fn != null)
				{
					var buffer = new byte[bytesReceived];
					_buffer.CopyTo(buffer, 0);
					var datagram = new UdpDatagram
					{
						Message = buffer,
						Sender = sender
					};
					fn(datagram);
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		#region IDisposable

		public void Dispose()
		{
			lock (_socket)
			{
				_socket.Dispose();
				_isDisposed = true;
			}
		}

		#endregion

		#region Implementation of IUdpSocket

		public event Action<UdpDatagram> OnMessage;

		#endregion
	}
}