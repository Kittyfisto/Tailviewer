using System.Collections.Generic;
using System.Xml;
using Tailviewer.BusinessLogic.Analysis.Analysers;

namespace Tailviewer.Settings.Dashboard
{
	/// <summary>
	///     Describes a single dashboard: Every layout and its widgets.
	/// </summary>
	public sealed class DashboardSettings
	{
		private readonly List<LayoutSettings> _layouts;
		private readonly List<LogAnalyserConfiguration> _analysers;

		public DashboardSettings()
		{
			_layouts = new List<LayoutSettings>();
			_analysers = new List<LogAnalyserConfiguration>();
		}

		public void Restore(XmlReader reader)
		{
			var layouts = new List<LayoutSettings>();
			var analysers = new List<LogAnalyserConfiguration>();
			XmlReader subtree = reader.ReadSubtree();

			while (subtree.Read())
			{
				switch (subtree.Name)
				{
					//case "dashboard": // "this"
					//	for (int i = 0; i < subtree.AttributeCount; ++i)
					//	{
					//		subtree.MoveToAttribute(i);
					//		switch (subtree.Name)
					//		{
					//		}
					//	}
					//	break;

					case "layout":
						var layout = new LayoutSettings();
						layout.Restore(reader);
						layouts.Add(layout);
						break;

					case "analyser":
						var analyser = LogAnalyserConfiguration.Restore(reader);
						analysers.Add(analyser);
						break;
				}
			}

			_layouts.Clear();
			_layouts.Capacity = layouts.Count;
			foreach (LayoutSettings layout in layouts)
			{
				_layouts.Add(layout);
			}

			_analysers.Clear();
			_analysers.Capacity = analysers.Count;
			foreach (LogAnalyserConfiguration analyser in analysers)
			{
				_analysers.Add(analyser);
			}
		}

		public void Save(XmlWriter writer)
		{
			foreach (LogAnalyserConfiguration widget in _analysers)
			{
				writer.WriteStartElement("analyser");
				widget.Save(writer);
				writer.WriteEndElement();
			}
			foreach (var layout in _layouts)
			{
				writer.WriteStartElement("layout");
				layout.Save(writer);
				writer.WriteEndElement();
			}
		}
	}
}