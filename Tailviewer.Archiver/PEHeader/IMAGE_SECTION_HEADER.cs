using System;
using System.Runtime.InteropServices;

namespace Tailviewer.Archiver.PEHeader
{
	/// <summary>
	/// From http://www.pinvoke.net/default.aspx/Structures/IMAGE_SECTION_HEADER.html
	/// via https://gist.github.com/caioproiete/b51f29f74f5f5b2c59c39e47a8afc3a3
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	// ReSharper disable once InconsistentNaming
	public struct IMAGE_SECTION_HEADER
	{
		[FieldOffset(0)]
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public char[] Name;
		[FieldOffset(8)]
		public UInt32 VirtualSize;
		[FieldOffset(12)]
		public UInt32 VirtualAddress;
		[FieldOffset(16)]
		public UInt32 SizeOfRawData;
		[FieldOffset(20)]
		public UInt32 PointerToRawData;
		[FieldOffset(24)]
		public UInt32 PointerToRelocations;
		[FieldOffset(28)]
		public UInt32 PointerToLinenumbers;
		[FieldOffset(32)]
		public UInt16 NumberOfRelocations;
		[FieldOffset(34)]
		public UInt16 NumberOfLinenumbers;
		[FieldOffset(36)]
		public DataSectionFlags Characteristics;

		public string Section
		{
			get { return new string(Name); }
		}
	}
}