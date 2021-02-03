using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.DataSources.UDP
{
	public sealed class UdpLogFile
		: AbstractLogFile
	{
		private readonly IUdpSocket _socket;
		private readonly ConcurrentQueue<UdpDatagram> _incomingDatagrams;
		private readonly LogFilePropertyList _properties;

		public UdpLogFile(ITaskScheduler scheduler,
						  Encoding encoding,
		                  IUdpSocket socket)
			: base(scheduler)
		{
			_properties = new LogFilePropertyList(LogFileProperties.Minimum);
			_properties.SetValue(LogFileProperties.Encoding, encoding);

			_socket = socket;
			_incomingDatagrams = new ConcurrentQueue<UdpDatagram>();

			// DO NOT PUT ANY MORE INITIALIZATION BELOW HERE OR THINGS WILL GO HORRIBLY WRONG
			_socket.OnMessage += OnMessageReceived;
		}

		private void OnMessageReceived(UdpDatagram datagram)
		{
			_incomingDatagrams.Enqueue(datagram);
		}

		#region Overrides of AbstractLogFile

		protected override void DisposeAdditional()
		{
			_socket.OnMessage -= OnMessageReceived;
			_socket?.Dispose();
		}

		public override int MaxCharactersPerLine
		{
			get { throw new NotImplementedException(); }
		}

		public override int Count
		{
			get { throw new NotImplementedException(); }
		}

		public override IReadOnlyList<ILogFileColumn> Columns
		{
			get { throw new NotImplementedException(); }
		}

		public override IReadOnlyList<ILogFilePropertyDescriptor> Properties
		{
			get { throw new NotImplementedException(); }
		}

		public override object GetValue(ILogFilePropertyDescriptor propertyDescriptor)
		{
			throw new NotImplementedException();
		}

		public override T GetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor)
		{
			throw new NotImplementedException();
		}

		public override void GetValues(ILogFileProperties properties)
		{
			throw new NotImplementedException();
		}

		public override void GetColumn<T>(LogFileSection section, ILogFileColumn<T> column, T[] buffer, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		public override void GetColumn<T>(IReadOnlyList<LogLineIndex> indices, ILogFileColumn<T> column, T[] buffer, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		public override void GetEntries(LogFileSection section, ILogEntries buffer, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		public override void GetEntries(IReadOnlyList<LogLineIndex> indices, ILogEntries buffer, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		public override void GetSection(LogFileSection section, LogLine[] dest)
		{
			throw new NotImplementedException();
		}

		public override LogLine GetLine(int index)
		{
			throw new NotImplementedException();
		}

		public override double Progress
		{
			get { throw new NotImplementedException(); }
		}

		protected override TimeSpan RunOnce(CancellationToken token)
		{
			const int maxOps = 1000;
			int numProcessed;
			for (numProcessed = 0; numProcessed < maxOps; ++numProcessed)
			{
				if (!_incomingDatagrams.TryDequeue(out var datagram))
					break;

				Add(datagram);
			}

			if (numProcessed > 0)
				return TimeSpan.Zero;

			return TimeSpan.FromMilliseconds(500);
		}

		private void Add(UdpDatagram datagram)
		{
			var message = TryDecode(datagram.Message);
		}

		private string TryDecode(byte[] datagram)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}