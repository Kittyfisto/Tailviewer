using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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

		public EventsLogAnalyserConfiguration Clone()
		{
			var clone = new EventsLogAnalyserConfiguration {MaxEvents = MaxEvents};
			clone.Events.AddRange(Events.Select(x => x.Clone()));
			return clone;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		public bool IsEquivalent(ILogAnalyserConfiguration other)
		{
			return false;
		}

		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Events", Events);
		}

		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Events", Events);
		}
	}
}