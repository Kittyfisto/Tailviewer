using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Tailviewer.Api;
using Tailviewer.BusinessLogic.Exporter;

namespace Tailviewer.BusinessLogic.ActionCenter
{
	public sealed class ExportAction
		: IExportAction
		, IProgress<Percentage>
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogFileToFileExporter _exporter;
		private readonly string _dataSourceName;
		private readonly string _exportDirectory;
		private Percentage _progress;
		private Exception _exception;
		private Task _task;

		public Exception Exception => _exception;

		public ExportAction(ILogFileToFileExporter exporter,
			string dataSourceName,
			string exportDirectory)
		{
			if (exporter == null)
				throw new ArgumentNullException(nameof(exporter));

			_exporter = exporter;
			_dataSourceName = dataSourceName;
			_exportDirectory = exportDirectory;
			_task = Task.Factory.StartNew(() => _exporter.Export(this))
				.ContinueWith(OnExported);
		}

		private void OnExported(Task task)
		{
			_task = null;
			if (task.IsFaulted)
			{
				_exception = TranslateException(task.Exception?.InnerException);
				Log.ErrorFormat("Error while exporting: {0}", _exception);
			}
			_progress = Percentage.HundredPercent;
		}

		private Exception TranslateException(Exception exception)
		{
			var dir = exception as DirectoryNotFoundException;
			if (dir != null)
			{
				var message = string.Format(
					"Unable to find or create directory '{0}'. Maybe its drive isn't connected or the medium is read only...",
					_exportDirectory);
				return new ExportException(message, exception);
			}

			return exception;
		}

		public string Title => "Exporting";

		/// <summary>
		/// </summary>
		/// <remarks>
		///     This action is used when the user wants to export a log file (or a portion thereof).
		///     In order to actuallly show the user that stuff is being exported, we force
		///     open the action center so the user sees a progress bar of the ongoing export (which may
		///     take a few seconds in case we're dealing with a multi-gb file).
		/// </remarks>
		public bool ForceShow => true;

		public Percentage Progress => _progress;

		public string DataSourceName => _dataSourceName;

		public string FullExportFilename => _exporter.FullExportFilename;

		public string Destination => _exportDirectory;

		void IProgress<Percentage>.Report(Percentage value)
		{
			if (_task == null)
				return;

			_progress = value;
		}
	}
}