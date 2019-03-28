using System;
using System.Runtime.Serialization;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Analysis.Count.Ui
{
	[DataContract]
	public sealed class LogEntryCountWidgetConfiguration
		: IWidgetConfiguration
	{
		public string Caption;

		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Caption", Caption);
		}

		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Caption", out Caption);
		}

		public LogEntryCountWidgetConfiguration Clone()
		{
			return new LogEntryCountWidgetConfiguration
			{
				Caption = Caption
			};
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}
