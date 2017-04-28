using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using log4net;
using Tailviewer.Settings.Dashboard.Analysers.Event;

namespace Tailviewer.Settings.Dashboard.Analysers
{
	public abstract class AnalyserSettings
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly Dictionary<string, Type> WidgetTypes;

		static AnalyserSettings()
		{
			WidgetTypes = new Dictionary<string, Type>();

			Add<CounterAnalyserSettings>();
			Add<EventsAnalyserSettings>();
			Add<QuickInfosAnalyserSettings>();
		}

		private static void Add<T>()
		{
			var type = typeof(T);
			WidgetTypes.Add(type.FullName, type);
		}

		public static AnalyserSettings Restore(XmlReader reader)
		{
			Type type = null;
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "type":
						type = GetType(reader.ReadContentAsString());
						break;
				}
			}

			if (type == null)
			{
				Log.ErrorFormat("Skipping unknown widget type: {0}", type);
				return null;
			}

			reader.MoveToElement();

			var widget = (AnalyserSettings)Activator.CreateInstance(type);
			widget.RestoreInternal(reader);
			return widget;
		}

		protected abstract void RestoreInternal(XmlReader reader);

		private static Type GetType(string fullName)
		{
			Type type;
			WidgetTypes.TryGetValue(fullName, out type);
			return type;
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("type", GetType().FullName);
			SaveInternal(writer);
		}

		protected abstract void SaveInternal(XmlWriter writer);
	}
}