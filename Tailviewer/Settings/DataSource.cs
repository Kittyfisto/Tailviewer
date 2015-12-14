using System.Xml;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Settings
{
	internal sealed class DataSource
	{
		public string File;
		public bool FollowTail;
		public bool IsOpen;
		public LevelFlags LevelFilter;
		public string StringFilter;
		public bool OtherFilter;
		public bool ColorByLevel;

		public DataSource()
		{
			LevelFilter = LevelFlags.All;
			OtherFilter = false;
			ColorByLevel = true;
		}

		public DataSource(string file)
		{
			File = file;
			LevelFilter = LevelFlags.All;
			OtherFilter = false;
			ColorByLevel = true;
		}


		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("file", File);
			writer.WriteAttributeBool("isopen", IsOpen);
			writer.WriteAttributeBool("followtail", FollowTail);
			writer.WriteAttributeString("stringfilter", StringFilter);
			writer.WriteAttributeEnum("levelfilter", LevelFilter);
			writer.WriteAttributeBool("otherfilter", OtherFilter);
			writer.WriteAttributeBool("colorbylevel", ColorByLevel);
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
						IsOpen = reader.ValueAsBool();
						break;

					case "followtail":
						FollowTail = reader.ValueAsBool();
						break;

					case "stringfilter":
						StringFilter = reader.Value;
						break;

					case "levelfilter":
						LevelFilter = reader.ReadContentAsEnum<LevelFlags>();
						break;

					case "otherfilter":
						OtherFilter = reader.ValueAsBool();
						break;

					case "colorbylevel":
						ColorByLevel = reader.ValueAsBool();
						break;
				}
			}
		}
	}
}