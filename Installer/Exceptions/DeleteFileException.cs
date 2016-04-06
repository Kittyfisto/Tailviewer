using System;

namespace Installer.Exceptions
{
	public class DeleteFileException
		: FileIoException
	{
		public DeleteFileException(string fileName, string sourceFolder, Exception innerException)
			: base(string.Format("Could not delete '{0}' from '{1}': {2}",
			                     fileName,
			                     sourceFolder,
			                     innerException.Message), innerException)
		{
		}
	}
}