using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using Metrolib;
using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer.Analysis.Events.BusinessLogic
{
	/// <summary>
	/// Contains all settings for a <see cref="EventsLogAnalyser"/>.
	/// Consists of mainly a list of <see cref="EventConfiguration"/>.
	/// </summary>
	[DataContract]
	public sealed class EventsLogAnalyserConfiguration
		: ILogAnalyserConfiguration
	{
		public readonly List<EventConfiguration> Events;
		public int MaxEvents;

		public EventsLogAnalyserConfiguration()
		{
			Events = new List<EventConfiguration>();
		}

		public void RestoreInternal(XmlReader reader)
		{
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "maxevents":
						MaxEvents = reader.ReadContentAsInt();
						break;
				}
			}

			reader.MoveToElement();
			XmlReader subtree = reader.ReadSubtree();

			while (subtree.Read())
			{
				switch (subtree.Name)
				{
					case "event":
						var @event = new EventConfiguration();
						@event.Restore(reader);
						Events.Add(@event);
						break;
				}
			}
		}

		public void SaveInternal(XmlWriter writer)
		{
			writer.WriteAttributeInt("maxevents", MaxEvents);

			foreach (var @event in Events)
			{
				writer.WriteStartElement("event");
				@event.Save(writer);
				writer.WriteEndElement();
			}
		}

		public object Clone()
		{
			throw new System.NotImplementedException();
		}

		public bool IsEquivalent(ILogAnalyserConfiguration other)
		{
			return false;
		}

		public void Serialize(IWriter writer)
		{
			throw new System.NotImplementedException();
		}

		public void Deserialize(IReader reader)
		{
			throw new System.NotImplementedException();
		}
	}
}