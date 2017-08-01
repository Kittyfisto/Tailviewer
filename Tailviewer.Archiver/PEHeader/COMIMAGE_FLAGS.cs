namespace Tailviewer.Archiver.PEHeader
{
	// ReSharper disable InconsistentNaming
	public enum COMIMAGE_FLAGS : uint
	{
		/// <summary>
		/// The image file contains IL code only, with no embedded native unmanaged code except the startup stub.
		/// Because common language runtime–aware operating systems (such as Windows XP) ignore the startup stub,
		/// for all practical purposes the file can be considered pure-IL.
		/// However, using this flag can cause certain ILAsm compiler–specific problems when running under Windows XP.
		/// If this flag is set, Windows XP ignores not only the startup stub but also the .reloc section.
		/// The .reloc section can contain relocations for the beginning and end of the .tls section as well as
		/// relocations for what is referred to as data-on-data (that is, data constants that are pointers to
		/// other data constants). Among existing managed compilers, only the MC++ compiler and linker
		/// and the ILAsm compiler can produce these items.
		/// The MC++ compiler and linker never set this flag because the image file they generate is never pure-IL.
		/// Currently, the ILAsm compiler is the only one capable of producing pure-IL image files that might
		/// require a .reloc section. To resolve this problem, the ILAsm compiler, if TLS-based data or data-on-data
		/// is emitted, clears this flag and sets the COMIMAGE_FLAGS_32BITREQUIRED flag instead.
		/// </summary>
		COMIMAGE_FLAGS_ILONLY = 0x00000001,

		/// <summary>
		/// The image file can be loaded only into a 32-bit process.
		/// This flag is set when native unmanaged code is embedded in the PE file
		/// or when the .reloc section is not empty.
		/// </summary>
		COMIMAGE_FLAGS_32BITREQUIRED = 0x00000002,

		/// <summary>
		/// This flag is obsolete and should not be set.
		/// Setting it—as the ILAsm compiler allows, using the .corflags directive—will render your module unloadable.
		/// </summary>
		COMIMAGE_FLAGS_IL_LIBRARY = 0x00000004,

		/// <summary>
		///  The image file is protected with a strong name signature.
		/// The strong name signature includes the public key and the signature hash
		/// and is a part of an assembly’s identity, along with the assembly name,
		/// version number, and the culture information.
		/// This flag is set when the strong name signing procedure is applied to the image file.
		/// No compiler, including ILAsm, can set this flag explicitly.
		/// </summary>
		COMIMAGE_FLAGS_STRONGNAMESIGNED = 0x00000008,

		/// <summary>
		/// The loader and the JIT (just-in-time) compiler are required to track debug information about the methods.
		/// </summary>
		COMIMAGE_FLAGS_TRACKDEBUGDATA = 0x00010000
	}
	// ReSharper restore InconsistentNaming
}