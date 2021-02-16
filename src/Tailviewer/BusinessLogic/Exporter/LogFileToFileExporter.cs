using System;
using System.IO;
using System.Text;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Exporter
{
	public sealed class LogFileToFileExporter
		: ILogFileToFileExporter
	{
		private readonly string _exportDirectory;
		private readonly ILogSource _logSource;
		private readonly string _dataSourceName;
		private string _fullExportFilename;

		public LogFileToFileExporter(ILogSource logSource,
			string exportDirectory,
			string dataSourceName)
		{
			if (logSource == null)
				throw new ArgumentNullException(nameof(logSource));

			_logSource = logSource;
			_exportDirectory = exportDirectory;
			_dataSourceName = dataSourceName;
		}
		
		public void Export(IProgress<Percentage> progressReporter = null)
		{
			using (var stream = OpenStream())
			{
				var exporter = new LogFileToStreamExporter(_logSource, stream);
				exporter.Export(progressReporter);
			}
		}

		public string FullExportFilename => _fullExportFilename;

		private Stream OpenStream()
		{
			if (!Directory.Exists(_exportDirectory))
				Directory.CreateDirectory(_exportDirectory);

			const int maxTries = 100;
			for (int i = 0; i < maxTries; ++i)
			{
				var sourceName = new StringBuilder();
				sourceName.Append(_exportDirectory);
				if (sourceName[sourceName.Length - 1] != '\\')
					sourceName.Append('\\');

				sourceName.Append(Path.GetFileNameWithoutExtension(_dataSourceName));
				if (i != 0)
					sourceName.AppendFormat(".{0}", i);
				sourceName.Append(".txt");

				var fname = sourceName.ToString();
				var stream = TryOpenStream(fname);
				if (stream != null)
				{
					_fullExportFilename = fname;
					return stream;
				}
			}

			return null;
		}

		private static FileStream TryOpenStream(string fileName)
		{
			if (File.Exists(fileName))
				return null;

			try
			{
				return File.Open(fileName, FileMode.CreateNew, FileAccess.Write, FileShare.None);
			}
			catch (DirectoryNotFoundException)
			{
				throw;
			}
			catch (IOException)
			{
				return null;
			}
		}
	}
}