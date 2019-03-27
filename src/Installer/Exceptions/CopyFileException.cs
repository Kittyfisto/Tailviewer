using System;

namespace Installer.Exceptions
{
	public class CopyFileException
		: FileIoException
	{
		public CopyFileException(string fileName, string destFolder, Exception innerException)
			: base(string.Format("Could not copy '{0}' to '{1}': {2}",
			                     fileName,
			                     destFolder,
			                     innerException.Message), innerException)
		{
		}
	}
}