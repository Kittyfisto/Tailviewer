using System;
using System.IO;
using System.Xml;

namespace Tailviewer.Settings
{
	internal sealed class ApplicationSettings
	{
		public static readonly ApplicationSettings Current;

		private readonly string _appDataLocal;
		private readonly string _fileName;

		private readonly WindowSettings _mainWindow;
		private readonly DataSources _dataSources;
		private readonly QuickFilters _quickFilters;

		static ApplicationSettings()
		{
			Current = new ApplicationSettings();
		}

		public ApplicationSettings()
		{
			_appDataLocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			_fileName = Path.Combine(_appDataLocal, Constants.ApplicationTitle, "settings");
			_fileName += ".xml";

			_mainWindow = new WindowSettings();
			_dataSources = new DataSources();
			_quickFilters = new QuickFilters();
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

		public bool Save()
		{
			return Save(_fileName);
		}

		internal bool Save(string fileName)
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

						writer.WriteEndElement();
					}

					using (var file = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
					{
						var length = (int) stream.Position;
						file.Write(stream.GetBuffer(), 0, length);
						file.SetLength(length);
					}

					return true;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

		internal void Restore(string fileName)
		{
			if (!File.Exists(fileName))
				return;

			try
			{
				using (FileStream stream = File.OpenRead(fileName))
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
								_dataSources.Restore(reader);
								break;

							case "quickfilters":
								_quickFilters.Restore(reader);
								break;
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}

		public void Restore()
		{
			Restore(_fileName);
		}
	}
}