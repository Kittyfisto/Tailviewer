using System;
using System.Diagnostics.Contracts;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Tailviewer.MMQ.V1;

namespace Tailviewer.MMQ
{
	public static class MemoryMappedQueue
	{
		/// <summary>
		///     Creates a new queue that can be accessed by any process on the same machine.
		///     Use <see cref="CreateProducer" /> and/or <see cref="CreateConsumer" /> in order to
		///     use the queue.
		/// </summary>
		/// <param name="name">A machine-wide unique name</param>
		/// <param name="bufferSize"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">When a queue with the same <paramref name="name" /> already exists</exception>
		/// <exception cref="ArgumentOutOfRangeException">When <paramref name="bufferSize" /> is too small </exception>
		public static IMemoryMappedQueue Create(string name, int bufferSize = ushort.MaxValue)
		{
			MemoryMappedFile file = null;
			try
			{
				file = MemoryMappedFile.CreateNew(name, bufferSize);
				var header = new Header
				{
					Cookie1 = Header.MagicCookieValue,
					Version = 1,
					FileLength = bufferSize,
					Cookie2 = Header.MagicCookieValue
				};

				using (var accessor = file.CreateViewAccessor())
				{
					unsafe
					{
						byte* start = null;
						accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref start);
						RtlZeroMemory(new IntPtr(start), (int) accessor.Capacity);
					}

					accessor.Write(0, ref header);
				}

				return new V1.MemoryMappedQueue(name, file);
			}
			catch (Exception)
			{
				file?.Dispose();
				throw;
			}
		}

		[DllImport("kernel32.dll")]
		static extern void RtlZeroMemory(IntPtr dst, int length);

		/// <summary>
		/// Creates a new queue that can be accessed by any process on the same machine.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static IMemoryMappedQueueProducer CreateProducer(string name)
		{
			MemoryMappedFile file = null;
			try
			{
				file = MemoryMappedFile.OpenExisting(name);
				var factory = CreateFactory(name, file);
				return factory.CreateProducer();
			}
			catch (Exception)
			{
				file?.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Creates a new queue that can be accessed by any process on the same machine.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static IMemoryMappedQueueConsumer CreateConsumer(string name)
		{
			MemoryMappedFile file = null;
			try
			{
				file = MemoryMappedFile.OpenExisting(name);
				var factory = CreateFactory(name, file);
				return factory.CreateConsumer();
			}
			catch(Exception)
			{
				file?.Dispose();
				throw;
			}
		}

		private static IMemoryMappedQueueFactory CreateFactory(string name, MemoryMappedFile file)
		{
			var headerSize = Marshal.SizeOf<Header>();
			using (var accessor = file.CreateViewAccessor(0, headerSize))
			{
				Header header;
				accessor.Read(0, out header);

				if (!header.Verify())
					throw new InvalidOperationException(string.Format("The given name '{0}' does not point to a valid memory mapped queue. This may be because the name is wrong, the queue's memory has been overwritten by another application or due to a bug in this library.", name));

				var dataSize = header.FileLength - headerSize;
				switch (header.Version)
				{
					case 1: return new MemoryMappedQueueFactory(name, file, headerSize, dataSize);
					default:
						throw new NotSupportedException(string.Format("MMQ of version '{0}' is not supported!", header.Version));
				}
			}
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct Header
		{
			public const long MagicCookieValue = 0xDEADFEED;
			public const int Size = 24;

			public long Cookie1;
			public int FileLength;
			public uint Version;
			public long Cookie2;

			[Pure]
			public bool Verify()
			{
				if (Cookie1 != MagicCookieValue)
					return false;
				if (FileLength <= Size)
					return false;
				if (Cookie2 != MagicCookieValue)
					return false;
				return true;
			}
		}
	}
}