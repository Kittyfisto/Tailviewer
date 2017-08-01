using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using log4net;

namespace Tailviewer.Archiver.PEHeader
{
	/// <summary>
	///     Reads in the header information of the Portable Executable format.
	///     Provides information such as the date the assembly was compiled.
	/// </summary>
	public class PeHeaderReader
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Private Fields

		/// <summary>
		///     The DOS header
		/// </summary>
		private readonly IMAGE_DOS_HEADER _dosHeader;

		/// <summary>
		///     The file header
		/// </summary>
		private IMAGE_FILE_HEADER _fileHeader;

		/// <summary>
		///     Optional 32 bit file header
		/// </summary>
		private readonly IMAGE_OPTIONAL_HEADER32? _peHeader32;

		/// <summary>
		///     Optional 64 bit file header
		/// </summary>
		private readonly IMAGE_OPTIONAL_HEADER64? _peHeader64;

		/// <summary>
		///     Image Section headers. Number of sections is in the file header.
		/// </summary>
		private readonly IMAGE_SECTION_HEADER[] _imageSectionHeaders;

		private readonly IMAGE_COR20_HEADER? _cliHeader;

		#endregion Private Fields

		#region Public Methods

		public PeHeaderReader(string filePath)
			: this(File.OpenRead(filePath))
		{}

		public PeHeaderReader(Stream stream, bool leaveOpen = false)
		{
			using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen))
			{
				_dosHeader = FromBinaryReader<IMAGE_DOS_HEADER>(reader);
				_dosHeader.ThrowIfInvalid();

				// Add 4 bytes to the offset
				stream.Seek(_dosHeader.e_lfanew, SeekOrigin.Begin);

				reader.ReadUInt32();
				_fileHeader = FromBinaryReader<IMAGE_FILE_HEADER>(reader);
				var peStart = stream.Position;
				ushort magic = reader.ReadUInt16();
				stream.Seek(-2, SeekOrigin.Current);
				switch (magic)
				{
					case 0x10b:
						_peHeader32 = FromBinaryReader<IMAGE_OPTIONAL_HEADER32>(reader);
						break;

					case 0x20b:
						_peHeader64 = FromBinaryReader<IMAGE_OPTIONAL_HEADER64>(reader);
						break;

					default:
						throw new InvalidOperationException(string.Format("Expected magic value of either 0x10b or 0x20b but found 0x{0:X}", magic));
				}

				_imageSectionHeaders = new IMAGE_SECTION_HEADER[_fileHeader.NumberOfSections];
				for (var headerNo = 0; headerNo < _imageSectionHeaders.Length; ++headerNo)
					_imageSectionHeaders[headerNo] = FromBinaryReader<IMAGE_SECTION_HEADER>(reader);

				if (CLRRuntimeHeader.Size > 0)
				{
					stream.Position = VirtualAddressToFilePosition(CLRRuntimeHeader.VirtualAddress, _imageSectionHeaders);
					_cliHeader = FromBinaryReader<IMAGE_COR20_HEADER>(reader);
				}
			}
		}

		/// <summary>
		///     Reads in a block from a file and converts it to the struct
		///     type specified by the template parameter
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reader"></param>
		/// <returns></returns>
		private static T FromBinaryReader<T>(BinaryReader reader)
		{
			// Read in a byte array
			var bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

			// Pin the managed memory while, copy it out the data, then unpin it
			var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			var theStructure = (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
			handle.Free();

			return theStructure;
		}

		#endregion Public Methods

		#region Properties

		/// <summary>
		///     Gets if the file header is 32 bit or not
		/// </summary>
		public bool Is32BitHeader => _peHeader32 != null;

		/// <summary>
		///     Gets the file header
		/// </summary>
		public IMAGE_FILE_HEADER FileHeader => _fileHeader;

		/// <summary>
		///     Gets the optional header
		/// </summary>
		public IMAGE_OPTIONAL_HEADER32 PeHeader32
		{
			get
			{
				if (_peHeader32 == null)
					throw new InvalidOperationException();

				return _peHeader32.Value;
			}
		}

		/// <summary>
		///     Gets the optional header
		/// </summary>
		public IMAGE_OPTIONAL_HEADER64 PeHeader64
		{
			get
			{
				if (_peHeader64 == null)
					throw new InvalidOperationException();

				return _peHeader64.Value;
			}
		}

		private IMAGE_DATA_DIRECTORY CLRRuntimeHeader
		{
			get
			{
				if (_peHeader32 != null)
				{
					return _peHeader32.Value.CLRRuntimeHeader;
				}
				return _peHeader64.Value.CLRRuntimeHeader;
			}
		}

		public bool IsClrAssembly => _cliHeader != null;

		public IMAGE_COR20_HEADER CliHeader
		{
			get
			{
				if (_cliHeader == null)
					throw new InvalidOperationException();

				return _cliHeader.Value;
			}
		}

		public IMAGE_DOS_HEADER DosHeader => _dosHeader;

		public IMAGE_SECTION_HEADER[] ImageSectionHeaders => _imageSectionHeaders;

		/// <summary>
		///     Gets the timestamp from the file header
		/// </summary>
		public DateTime TimeStamp
		{
			get
			{
				// Timestamp is a date offset from 1970
				var returnValue = new DateTime(1970, 1, 1, 0, 0, 0);

				// Add in the number of seconds since 1970/1/1
				returnValue = returnValue.AddSeconds(_fileHeader.TimeDateStamp);
				// Adjust to local timezone
				returnValue += TimeZone.CurrentTimeZone.GetUtcOffset(returnValue);

				return returnValue;
			}
		}

		private static long VirtualAddressToFilePosition(uint virtualAddress, IMAGE_SECTION_HEADER[] sectionHeaders)
		{
			for (int i = 0; i < sectionHeaders.Length; ++i)
			{
				long relativeVirtualAddress = (long)virtualAddress - sectionHeaders[i].VirtualAddress;
				if (relativeVirtualAddress >= 0 && relativeVirtualAddress < sectionHeaders[i].SizeOfRawData)
					return sectionHeaders[i].PointerToRawData + relativeVirtualAddress;
			}
			throw new InvalidDataException(string.Format("Could not resolve virtual address 0x{0:X}", virtualAddress));
		}

		#endregion Properties

		public static bool TryReadFrom(Stream stream, out PeHeaderReader reader, bool leaveOpen = false)
		{
			try
			{
				reader = new PeHeaderReader(stream, leaveOpen);
				return true;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught exception: {0}", e);

				reader = null;
				return false;
			}
		}
	}
}