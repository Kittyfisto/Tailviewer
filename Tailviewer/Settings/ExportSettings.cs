using System;
using System.Xml;

namespace Tailviewer.Settings
{
	public sealed class ExportSettings
		: ICloneable
	{
		public string ExportFolder;

		public ExportSettings Clone()
		{
			return new ExportSettings
			{
				ExportFolder = ExportFolder
			};
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("exportfolder", ExportFolder);
		}

		public void Restore(XmlReader reader)
		{
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "exportfolder":
						ExportFolder = reader.ReadContentAsString();
						break;
				}
			}
		}
	}
}