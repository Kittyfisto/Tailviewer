using System.Runtime.InteropServices;

namespace Tailviewer.Archiver.PEHeader
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	// ReSharper disable once InconsistentNaming
	public struct IMAGE_FILE_HEADER
	{
		public MACHINE Machine;
		public ushort NumberOfSections;
		public uint TimeDateStamp;
		public uint PointerToSymbolTable;
		public uint NumberOfSymbols;
		public ushort SizeOfOptionalHeader;
		public CHARACTERISTICS Characteristics;
	}
}