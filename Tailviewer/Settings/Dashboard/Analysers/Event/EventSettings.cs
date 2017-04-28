using System.Xml;
using Metrolib;
using Tailviewer.BusinessLogic.Dashboard.Analysers;

namespace Tailviewer.Settings.Dashboard.Analysers.Event
{
	/// <summary>
	/// Contains the settings for a single event of a <see cref="EventsLogAnalyser"/>.
	/// </summary>
	public sealed class EventSettings
	{
		public string Name;
		public bool IgnoreCase;
		public QuickFilterMatchType FilterType;
		public string FilterExpression;
		public string DisplayExpression;

		public void Restore(XmlReader reader)
		{
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "name":
						Name = reader.ReadContentAsString();
						break;
					case "ignorecase":
						IgnoreCase = reader.ReadContentAsBool();
						break;
					case "filterexpression":
						FilterExpression = reader.ReadContentAsString();
						break;
					case "filtertype":
						FilterType = reader.ReadContentAsEnum<QuickFilterMatchType>();
						break;
					case "displayexpression":
						DisplayExpression = reader.ReadContentAsString();
						break;
				}
			}
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("name", Name);
			writer.WriteAttributeBool("ignorecase", IgnoreCase);
			writer.WriteAttributeString("filterexpression", FilterExpression);
			writer.WriteAttributeEnum("filtertype", FilterType);
			writer.WriteAttributeString("displayexpression", DisplayExpression);
		}
	}
}