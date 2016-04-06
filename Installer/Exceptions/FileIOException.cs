using System;
using System.IO;

namespace Installer.Exceptions
{
	public class FileIoException
		: IOException
	{
		public FileIoException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}