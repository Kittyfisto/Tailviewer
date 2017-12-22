using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Tailviewer.BusinessLogic.FileExplorer;
using Tailviewer.Core;

namespace Tailviewer.BusinessLogic.ActionCenter
{
	public sealed class OpenFolderAction
		: IOpenFolderAction
			, IProgress<Percentage>
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private Task _task;
		private readonly IFileExplorer _fileExplorer;

		public OpenFolderAction(string path, IFileExplorer fileExplorer)
		{

			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(nameof(path));

			if(fileExplorer == null)
				throw new ArgumentNullException(nameof(fileExplorer));

			_fileExplorer = fileExplorer;

			_task = Task.Run(() =>
			{
				if (!FileEx.Exists(path, TimeSpan.FromSeconds(2)))
				{
					FullFoldername = FileEx.FindClosestExistingFolder(path);
					_fileExplorer.OpenFolder(FullFoldername, this);
				}
				else
				{
					FullFoldername = path;
					_fileExplorer.SelectFile(path, this);
				}
			})
				.ContinueWith(OnFolderOpened);
		}

		public string Title => "Opening Folder In Explorer";
		public bool ForceShow => true;
		public Percentage Progress { get; private set; }
		public Exception Exception { get; private set; }
		public string FullFoldername { get; set; }

		void IProgress<Percentage>.Report(Percentage value)
		{
			if (_task == null)
				return;

			Progress = value;
		}

		private void OnFolderOpened(Task task)
		{
			_task = null;
			if (task.IsFaulted)
			{
				Exception = TranslateException(task.Exception?.InnerException);
				Log.ErrorFormat("Error while opening folder: {0}", Exception);
			}
			Progress = Percentage.HundredPercent;
		}

		private Exception TranslateException(Exception exception)
		{
			var dir = exception as DirectoryNotFoundException;
			if (dir != null)
			{
				var message = string.Format(
					"Unable to find directory '{0}'.",
					FullFoldername);
				return new OpenFolderException(message, exception);
			}

			return exception;
		}
	}
}