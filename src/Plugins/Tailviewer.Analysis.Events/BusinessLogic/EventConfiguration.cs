using System.Runtime.Serialization;
using System.Xml;

namespace Tailviewer.Analysis.Events.BusinessLogic
{
	/// <summary>
	/// Contains the settings for a single event of a <see cref="EventsLogAnalyser"/>.
	/// </summary>
	[DataContract]
	public sealed class EventConfiguration
		: ISerializableType
	{
		public string Name;
		public string FilterExpression;

		public override string ToString()
		{
			return string.Format("{0}, {1}", Name, FilterExpression);
		}

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
					case "filterexpression":
						FilterExpression = reader.ReadContentAsString();
						break;
				}
			}
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("name", Name);
			writer.WriteAttributeString("filterexpression", FilterExpression);
		}

		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("name", Name);
			writer.WriteAttribute("filterexpression", FilterExpression);
		}

		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("name", out Name);
			reader.TryReadAttribute("filterexpression", out FilterExpression);
		}

		public EventConfiguration Clone()
		{
			return new EventConfiguration
			{
				Name = Name,
				FilterExpression = FilterExpression
			};
		}
	}
}