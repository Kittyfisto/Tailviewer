using System;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace Tailviewer.MMQ.V1
{
	internal abstract class MemoryMappedQueueAccessor
		: IDisposable
	{
		/// <summary>
		/// Represents a held, exclusive lock on a queue.
		/// </summary>
		interface IQueueLock
			: IDisposable
		{}

		/// <summary>
		///     A system-wide lock that allows exclusive read/write access to the queue.
		/// </summary>
		/// <remarks>
		///     The memory mapped file looks as follows:
		///     -------------------------------------------------------------
		///     |--Read Pointer--|--Write Pointer--|------Data Segment------|
		///     -------------------------------------------------------------
		///     Data segment has a length of <see cref="_dataLength" /> and both read- and write pointer
		///     may take values from 0 to <see cref="_dataLength" />.
		/// </remarks>
		internal sealed class QueueLock
			: IQueueLock
		{
			private const int ReadPointerOffset = 0;
			private const int WritePointerOffset = 4;
			private const int DataOffset = 8;
			private const int MessageStartCookie = 0xAFFE;
			private const int MessageEndCookie = 0xC01D;

			private readonly MemoryMappedViewAccessor _accessor;
			private readonly int _length;
			private readonly int _dataLength;
			private readonly Mutex _mutex;
			private bool _acquired;

			/// <summary>
			/// 
			/// </summary>
			/// <remarks>
			/// Does NOT take ownership of the given accessor.
			/// </remarks>
			/// <param name="accessor"></param>
			/// <param name="queueName"></param>
			public QueueLock(MemoryMappedViewAccessor accessor, string queueName)
			{
				_accessor = accessor;
				_length = (int)accessor.Capacity;
				_dataLength = _length - DataOffset;
				_mutex = new Mutex(false, string.Format("{0}.Mutex", queueName));
			}

			public bool Acquire(int millisecondsTimeout)
			{
				return _acquired = _mutex.WaitOne(millisecondsTimeout);
			}

			private int ReadPointer
			{
				get { return _accessor.ReadInt32(ReadPointerOffset); }
				set { _accessor.Write(ReadPointerOffset, value); }
			}

			private int WritePointer
			{
				get { return _accessor.ReadInt32(WritePointerOffset); }
				set { _accessor.Write(WritePointerOffset, value); }
			}

			public int AvailableWriteLength
			{
				get
				{
					var r = ReadPointer;
					var w = WritePointer;

					if (r < w)
						return _length - w + r;

					if (w == r)
						return _length;

					// r > w
					return r - w;
				}
			}

			public int AvailableReadLength
			{
				get
				{
					var r = ReadPointer;
					var w = WritePointer;

					if (r < w)
						return w - r;

					if (r == w)
						return 0;

					// r > w
					return _length - r + w;
				}
			}

			public byte[] ReadMessage()
			{
				var r = ReadPointer;
				var cookie1 = Read(ref r);
				var length = Read(ref r);
				var data = ReadArray(ref r, length);
				var cookie2 = Read(ref r, align: true);
				ReadPointer = r;

				return data;
			}

			private int Read(ref int readPointer, bool align = false)
			{
				var value = _accessor.ReadInt32(DataOffset + readPointer);
				readPointer = (readPointer + 4) % _dataLength;

				if (align)
				{
					var offset = 4 - readPointer % 4;
					readPointer += offset;
				}

				return value;
			}

			private byte[] ReadArray(ref int readPointer, int length)
			{
				var data = new byte[length];

				var remaining = _dataLength - readPointer;
				if (remaining < length)
				{
					_accessor.ReadArray(DataOffset + readPointer, data, 0, remaining);
					_accessor.ReadArray(DataOffset, data, remaining, length - remaining);
					readPointer = length - remaining;
				}
				else
				{
					_accessor.ReadArray(DataOffset + readPointer, data, 0, length);
					readPointer += length;
				}

				return data;
			}

			/// <summary>
			/// Writes a message to the data portion of the queue.
			/// </summary>
			/// <param name="data"></param>
			public void WriteMessage(byte[] data)
			{
				var r = WritePointer;
				Write(ref r, MessageStartCookie);
				Write(ref r, data.Length);
				Write(ref r, data);
				Write(ref r, MessageEndCookie, align: true);
				WritePointer = r;
			}

			private void Write(ref int writePointer, int value, bool align = false)
			{
				_accessor.Write(DataOffset + writePointer, value);
				writePointer = (writePointer + 4) % _dataLength;

				if (align)
				{
					var offset = 4 - writePointer % 4;
					writePointer += offset;
				}
			}

			private void Write(ref int writePointer, byte[] data)
			{
				var remaining = _dataLength - writePointer;
				if (remaining < data.Length)
				{
					_accessor.WriteArray(DataOffset + writePointer, data, 0, remaining);
					_accessor.WriteArray(DataOffset, data, remaining, data.Length - remaining);
					writePointer = DataOffset + data.Length - remaining;
				}
				else
				{
					_accessor.WriteArray(DataOffset + writePointer, data, 0, data.Length);
					writePointer = writePointer + data.Length;
				}
			}

			public void Dispose()
			{
				if (_acquired)
					_mutex.ReleaseMutex();
				_mutex?.Dispose();
			}
		}

		public abstract void Dispose();
	}
}