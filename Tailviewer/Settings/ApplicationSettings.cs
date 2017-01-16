using System;
using System.IO;
using System.Reflection;
using System.Xml;
using Metrolib.Settings;
using log4net;

namespace Tailviewer.Settings
{
	public sealed class ApplicationSettings
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly AutoUpdateSettings _autoUpdate;

		private readonly DataSources _dataSources;
		private readonly string _fileFolder;
		private readonly string _fileName;
		private readonly WindowSettings _mainWindow;
		private readonly QuickFilters _quickFilters;

		public ApplicationSettings(string fileName)
		{
			_fileName = Path.GetFullPath(fileName);
			_fileFolder = Path.GetDirectoryName(_fileName);

			_autoUpdate = new AutoUpdateSettings();
			_mainWindow = new WindowSettings();
			_dataSources = new DataSources();
			_quickFilters = new QuickFilters();
		}

		public AutoUpdateSettings AutoUpdate
		{
			get { return _autoUpdate; }
		}

		public WindowSettings MainWindow
		{
			get { return _mainWindow; }
		}

		public DataSources DataSources
		{
			get { return _dataSources; }
		}

		public QuickFilters QuickFilters
		{
			get { return _quickFilters; }
		}

		public static ApplicationSettings Create()
		{
			string fileName = Path.Combine(Constants.AppDataLocalFolder, "settings");
			fileName += ".xml";
			return new ApplicationSettings(fileName);
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
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}
	}
}