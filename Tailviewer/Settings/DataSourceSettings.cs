using System;
using System.Xml;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Settings
{
	internal sealed class DataSourceSettings
	{
		public string File;
		public bool FollowTail;
		public bool IsOpen;
		public LevelFlags LevelFilter;
		public string StringFilter;

		public DataSourceSettings()
		{}

		public DataSourceSettings(string file)
		{
			File = file;
			LevelFilter = LevelFlags.All;
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("file", File);
			writer.WriteAttributeString("isopen", IsOpen ? "true" : "false");
			writer.WriteAttributeString("followtail", FollowTail ? "true" : "false");
			writer.WriteAttributeString("stringfilter", StringFilter);
			writer.WriteAttributeString("levelfilter", LevelFilter.ToString());
		}

		public void Restore(XmlReader reader)
		{
			int count = reader.AttributeCount;
			for (int i = 0; i < count; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "file":
						File = reader.Value;
						break;

					case "isopen":
						IsOpen = reader.Value == "true";
						break;

					case "followtail":
						FollowTail = reader.Value == "true";
						break;

					case "stringfilter":
						StringFilter = reader.Value;
						break;

					case "levelfilter":
						LevelFilter = (LevelFlags) Enum.Parse(typeof (LevelFlags), reader.Value);
						break;
				}
			}
		}
	}
}