using System;
using System.Runtime.InteropServices;

namespace Tailviewer.Archiver.PEHeader
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	// ReSharper disable once InconsistentNaming
	public struct IMAGE_DOS_HEADER
	{      // DOS .EXE header
		public UInt16 e_magic;              // Magic number
		public UInt16 e_cblp;               // Bytes on last page of file
		public UInt16 e_cp;                 // Pages in file
		public UInt16 e_crlc;               // Relocations
		public UInt16 e_cparhdr;            // Size of header in paragraphs
		public UInt16 e_minalloc;           // Minimum extra paragraphs needed
		public UInt16 e_maxalloc;           // Maximum extra paragraphs needed
		public UInt16 e_ss;                 // Initial (relative) SS value
		public UInt16 e_sp;                 // Initial SP value
		public UInt16 e_csum;               // Checksum
		public UInt16 e_ip;                 // Initial IP value
		public UInt16 e_cs;                 // Initial (relative) CS value
		public UInt16 e_lfarlc;             // File address of relocation table
		public UInt16 e_ovno;               // Overlay number
		public UInt16 e_res_0;              // Reserved words
		public UInt16 e_res_1;              // Reserved words
		public UInt16 e_res_2;              // Reserved words
		public UInt16 e_res_3;              // Reserved words
		public UInt16 e_oemid;              // OEM identifier (for e_oeminfo)
		public UInt16 e_oeminfo;            // OEM information; e_oemid specific
		public UInt16 e_res2_0;             // Reserved words
		public UInt16 e_res2_1;             // Reserved words
		public UInt16 e_res2_2;             // Reserved words
		public UInt16 e_res2_3;             // Reserved words
		public UInt16 e_res2_4;             // Reserved words
		public UInt16 e_res2_5;             // Reserved words
		public UInt16 e_res2_6;             // Reserved words
		public UInt16 e_res2_7;             // Reserved words
		public UInt16 e_res2_8;             // Reserved words
		public UInt16 e_res2_9;             // Reserved words
		public UInt32 e_lfanew;             // File address of new exe header

		public void ThrowIfInvalid()
		{
			if (e_magic != 0x5a4d)
				throw new Exception(string.Format("Expected first WORD to be 0x5a4d, but found {0:X}", e_magic));
		}
	}
}