using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using log4net;
using Tailviewer.BusinessLogic.Analysis.Analysers.Count;
using Tailviewer.BusinessLogic.Analysis.Analysers.Event;

namespace Tailviewer.BusinessLogic.Analysis.Analysers
{
	public abstract class LogAnalyserConfiguration
		: ILogAnalyserConfiguration
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly Dictionary<string, Type> WidgetTypes;

		static LogAnalyserConfiguration()
		{
			WidgetTypes = new Dictionary<string, Type>();

			Add<LogEntryCountAnalyserConfiguration>();
			Add<EventsLogAnalyserConfiguration>();
		}

		private static void Add<T>()
		{
			var type = typeof(T);
			WidgetTypes.Add(type.FullName, type);
		}

		public static LogAnalyserConfiguration Restore(XmlReader reader)
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

			var widget = (LogAnalyserConfiguration)Activator.CreateInstance(type);
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

		public abstract object Clone();
	}
}