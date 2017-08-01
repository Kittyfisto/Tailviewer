using System;
using System.Runtime.InteropServices;

namespace Tailviewer.Archiver.PEHeader
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	// ReSharper disable once InconsistentNaming
	public struct IMAGE_OPTIONAL_HEADER32
	{
		public UInt16 Magic;
		public Byte MajorLinkerVersion;
		public Byte MinorLinkerVersion;
		public UInt32 SizeOfCode;
		public UInt32 SizeOfInitializedData;
		public UInt32 SizeOfUninitializedData;
		public UInt32 AddressOfEntryPoint;
		public UInt32 BaseOfCode;
		public UInt32 BaseOfData;
		public UInt32 ImageBase;
		public UInt32 SectionAlignment;
		public UInt32 FileAlignment;
		public UInt16 MajorOperatingSystemVersion;
		public UInt16 MinorOperatingSystemVersion;
		public UInt16 MajorImageVersion;
		public UInt16 MinorImageVersion;
		public UInt16 MajorSubsystemVersion;
		public UInt16 MinorSubsystemVersion;
		public UInt32 Win32VersionValue;
		public UInt32 SizeOfImage;
		public UInt32 SizeOfHeaders;
		public UInt32 CheckSum;
		public SUBSYSTEM Subsystem;
		public UInt16 DllCharacteristics;
		public UInt32 SizeOfStackReserve;
		public UInt32 SizeOfStackCommit;
		public UInt32 SizeOfHeapReserve;
		public UInt32 SizeOfHeapCommit;
		public UInt32 LoaderFlags;
		public UInt32 NumberOfRvaAndSizes;

		public IMAGE_DATA_DIRECTORY ExportTable;
		public IMAGE_DATA_DIRECTORY ImportTable;
		public IMAGE_DATA_DIRECTORY ResourceTable;
		public IMAGE_DATA_DIRECTORY ExceptionTable;
		public IMAGE_DATA_DIRECTORY CertificateTable;
		public IMAGE_DATA_DIRECTORY BaseRelocationTable;
		public IMAGE_DATA_DIRECTORY Debug;
		public IMAGE_DATA_DIRECTORY Architecture;
		public IMAGE_DATA_DIRECTORY GlobalPtr;
		public IMAGE_DATA_DIRECTORY TLSTable;
		public IMAGE_DATA_DIRECTORY LoadConfigTable;
		public IMAGE_DATA_DIRECTORY BoundImport;
		public IMAGE_DATA_DIRECTORY IAT;
		public IMAGE_DATA_DIRECTORY DelayImportDescriptor;
		public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;
		public IMAGE_DATA_DIRECTORY Reserved;
	}
}