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
				if (!FileEx.Exists(path, TimeSpan.FromSeconds(1)))
				{
					FullFoldername = FileEx.FindClosestExistingFolder(path);
					_fileExplorer.OpenFolder(FullFoldername);
				}
				else
				{
					FullFoldername = path;
					_fileExplorer.SelectFile(path);
				}
			})
				.ContinueWith(OnFolderOpened);
		}

		public OpenFolderAction(string[] files, string folder, IFileExplorer fileExplorer)
		{
			if (files == null)
				throw new ArgumentNullException(nameof(files));

			if (string.IsNullOrEmpty(folder))
				throw new ArgumentNullException(nameof(folder));

			if (fileExplorer == null)
				throw new ArgumentNullException(nameof(fileExplorer));

			_fileExplorer = fileExplorer;

			_task = Task.Run(() =>
			{
				if (!Directory.Exists(folder))
				{
					FullFoldername = FileEx.FindClosestExistingFolder(folder);
					_fileExplorer.OpenFolder(FullFoldername);
				}
				else
				{
					FullFoldername = folder;
					_fileExplorer.SelectFiles(FullFoldername, files);
				}
			})
				.ContinueWith(OnFolderOpened);
		}

		public string Title => "Opening Folder In Explorer";
		public bool ForceShow { get; set; }
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
				Exception = task.Exception?.InnerException;
				ForceShow = true;
				Log.ErrorFormat("Error while opening folder: {0}", Exception);
			}
			Progress = Percentage.HundredPercent;
		}
	}
}