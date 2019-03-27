using System;
using System.Xml;

namespace Tailviewer.Settings
{
	public sealed class ExportSettings
		: IExportSettings
		, ICloneable
	{
		private string _exportFolder;

		public ExportSettings Clone()
		{
			return new ExportSettings
			{
				_exportFolder = _exportFolder
			};
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("exportfolder", _exportFolder);
		}

		public void Restore(XmlReader reader)
		{
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "exportfolder":
						_exportFolder = reader.ReadContentAsString();
						
						break;
				}
			}

			if (string.IsNullOrEmpty(_exportFolder))
			{
				_exportFolder = Constants.ExportDirectory;
			}
		}

		public string ExportFolder
		{
			get { return _exportFolder; }
			set { _exportFolder = value; }
		}
	}
}