using System;

namespace Tailviewer.BusinessLogic.ActionCenter
{
	public sealed class OpenFolderException : Exception
	{
		public OpenFolderException(string message) : base(message)
		{}
		public OpenFolderException(string message, Exception inner)
			: base(message, inner)
		{ }
	}
}