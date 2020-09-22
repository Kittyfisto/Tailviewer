using System.Collections.Generic;
using System.Xml;

namespace Tailviewer.Settings.CustomFormats
{
	internal sealed class CustomFormatsSettings
		: List<CustomLogFileFormat>
		, ICustomFormatsSettings
	{
		public CustomFormatsSettings()
		{ }

		private CustomFormatsSettings(CustomFormatsSettings customFormatsSettings)
			: base(customFormatsSettings)
		{}

		public CustomFormatsSettings Clone()
		{
			return new CustomFormatsSettings(this);
		}

		public void Save(XmlWriter writer)
		{
			foreach (var customLogFileFormat in this)
			{
				writer.WriteStartElement("customformat");
				customLogFileFormat.Save(writer);
				writer.WriteEndElement();
			}
		}

		public void Restore(XmlReader reader)
		{
			var customFormats = new List<CustomLogFileFormat>();
			var subtree = reader.ReadSubtree();
			while (subtree.Read())
				switch (subtree.Name)
				{
					case "customformat":
						var dataSource = new CustomLogFileFormat();
						dataSource.Restore(subtree);
						customFormats.Add(dataSource);
						break;
				}

			Clear();
			Capacity = customFormats.Count;
			foreach (var source in customFormats)
			{
				Add(source);
			}
		}
	}
}