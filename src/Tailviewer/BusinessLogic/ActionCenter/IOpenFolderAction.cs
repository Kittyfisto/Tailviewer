using System;

namespace Tailviewer.BusinessLogic.ActionCenter
{
	public interface IOpenFolderAction 
		: INotification
	{
		Exception Exception { get; }
		Percentage Progress { get; }
		string FullFoldername { get; }
	}
}