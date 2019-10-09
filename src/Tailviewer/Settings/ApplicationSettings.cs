using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using log4net;
using Tailviewer.Core.Settings;

namespace Tailviewer.Settings
{
	public sealed class ApplicationSettings
		: IApplicationSettings
		, ICloneable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static Task _saveTask;

		private readonly AutoUpdateSettings _autoUpdate;

		private readonly DataSourceSettings _dataSources;
		private readonly string _fileFolder;
		private readonly string _fileName;
		private readonly MainWindowSettings _mainWindow;
		private readonly QuickFilters _quickFilters;
		private readonly ExportSettings _export;
		private readonly LogViewerSettings _logViewer;
		private readonly LogFileSettings _logFile;
		private int _numSaved;

		/// <summary>
		///    How often <see cref=" SaveAsync"/> was called.
		/// </summary>
		public int NumSaved
		{
			get { return _numSaved; }
		}

		static ApplicationSettings()
		{
			_saveTask = Task.FromResult(42);
		}

		private ApplicationSettings(ApplicationSettings other)
		{
			_fileName = other._fileName;
			_fileFolder = other._fileFolder;
			_autoUpdate = other._autoUpdate.Clone();
			_mainWindow = other._mainWindow.Clone();
			_dataSources = other._dataSources.Clone();
			_quickFilters = other._quickFilters.Clone();
			_export = other._export.Clone();
			_logViewer = other._logViewer.Clone();
			_logFile = other._logFile.Clone();
		}

		public ApplicationSettings(string fileName)
		{
			_fileName = Path.GetFullPath(fileName);
			_fileFolder = Path.GetDirectoryName(_fileName);

			_autoUpdate = new AutoUpdateSettings();
			_mainWindow = new MainWindowSettings();
			_dataSources = new DataSourceSettings();
			_quickFilters = new QuickFilters();
			_logViewer = new LogViewerSettings();
			_logFile = new LogFileSettings();
			_export = new ExportSettings();
		}

		public IAutoUpdateSettings AutoUpdate => _autoUpdate;

		public IMainWindowSettings MainWindow => _mainWindow;

		public IDataSourcesSettings DataSources => _dataSources;

		public ILogViewerSettings LogViewer => _logViewer;

		public ILogFileSettings LogFile => _logFile;

		public QuickFilters QuickFilters => _quickFilters;

		public IExportSettings Export => _export;

		public static ApplicationSettings Create()
		{
			string fileName = Path.Combine(Constants.AppDataLocalFolder, "settings");
			fileName += ".xml";
			return new ApplicationSettings(fileName);
		}

		public void SaveAsync()
		{
			var config = Clone();
			_saveTask = _saveTask.ContinueWith(unused => config.Save());
			++_numSaved;
		}

		public bool Save()
		{
			try
			{
				using (var stream = new MemoryStream())
				{
					var settings = new XmlWriterSettings
						{
							Indent = true,
							IndentChars = "  ",
							NewLineChars = "\r\n",
							NewLineHandling = NewLineHandling.Replace
						};
					using (XmlWriter writer = XmlWriter.Create(stream, settings))
					{
						writer.WriteStartElement("xml");

						writer.WriteStartElement("mainwindow");
						_mainWindow.Save(writer);
						writer.WriteEndElement();

						writer.WriteStartElement("datasources");
						_dataSources.Save(writer);
						writer.WriteEndElement();

						writer.WriteStartElement("quickfilters");
						_quickFilters.Save(writer);
						writer.WriteEndElement();

						writer.WriteStartElement("autoupdate");
						_autoUpdate.Save(writer);
						writer.WriteEndElement();

						writer.WriteStartElement("export");
						_export.Save(writer);
						writer.WriteEndElement();

						writer.WriteStartElement("logviewer");
						_logViewer.Save(writer);
						writer.WriteEndElement();

						writer.WriteStartElement("logfile");
						_logFile.Save(writer);
						writer.WriteEndElement();

						writer.WriteEndElement();
					}

					if (!Directory.Exists(_fileFolder))
						Directory.CreateDirectory(_fileFolder);

					using (var file = new FileStream(_fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
					{
						var length = (int) stream.Position;
						file.Write(stream.GetBuffer(), 0, length);
						file.SetLength(length);
					}

					return true;
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return false;
			}
		}

		public void Restore()
		{
			bool unused;
			Restore(out unused);
		}

		/// <summary>
		/// </summary>
		/// <param name="neededPatching">Whether or not certain values need to be changed (for example due to upgrades to the format - it is advised that the current settings be saved again if this is set to true)</param>
		public void Restore(out bool neededPatching)
		{
			neededPatching = false;
			if (!File.Exists(_fileName))
				return;

			try
			{
				using (FileStream stream = File.OpenRead(_fileName))
				using (XmlReader reader = XmlReader.Create(stream))
				{
					while (reader.Read())
					{
						switch (reader.Name)
						{
							case "mainwindow":
								_mainWindow.Restore(reader);
								break;

							case "datasources":
								bool dataSourcesNeededPatching;
								_dataSources.Restore(reader, out dataSourcesNeededPatching);
								neededPatching |= dataSourcesNeededPatching;
								break;

							case "quickfilters":
								_quickFilters.Restore(reader);
								break;

							case "autoupdate":
								_autoUpdate.Restore(reader);
								break;

							case "export":
								_export.Restore(reader);
								break;

							case "logviewer":
								_logViewer.Restore(reader);
								break;

							case "logfile":
								_logFile.Restore(reader);
								break;
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		public ApplicationSettings Clone()
		{
			return new ApplicationSettings(this);
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}