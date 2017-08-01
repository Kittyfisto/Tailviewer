using System.Runtime.InteropServices;

namespace Tailviewer.Archiver.PEHeader
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	// ReSharper disable InconsistentNaming
	public struct IMAGE_COR20_HEADER
	{
		public uint cb;
		public ushort MajorRuntimeVersion;
		public ushort MinorRuntimeVersion;
		public IMAGE_DATA_DIRECTORY MetaData;
		public COMIMAGE_FLAGS Flags;
		public uint EntryPointToken;
		public IMAGE_DATA_DIRECTORY Resources;
		public IMAGE_DATA_DIRECTORY StrongNameSignature;
		public IMAGE_DATA_DIRECTORY CodeManagerTable;
		public IMAGE_DATA_DIRECTORY VTableFixups;
		public IMAGE_DATA_DIRECTORY ExportAddressTableJumps;
		public IMAGE_DATA_DIRECTORY ManagedNativeHeader;
	}
	// ReSharper restore InconsistentNaming
}
