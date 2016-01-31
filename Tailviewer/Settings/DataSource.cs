using System;
using System.Collections.Generic;
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
		public bool ColorByLevel;
		public LogLineIndex SelectedLogLine;
		public LogLineIndex VisibleLogLine;
		private readonly List<Guid> _activatedQuickFilters;

		public List<Guid> ActivatedQuickFilters
		{
			get { return _activatedQuickFilters; }
		}

		public DataSource()
		{
			_activatedQuickFilters = new List<Guid>();
			LevelFilter = LevelFlags.All;
			ColorByLevel = true;
			SelectedLogLine = LogLineIndex.Invalid;
			VisibleLogLine = LogLineIndex.Invalid;
		}

		public DataSource(string file)
			: this()
		{
			File = file;
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("file", File);
			writer.WriteAttributeBool("isopen", IsOpen);
			writer.WriteAttributeBool("followtail", FollowTail);
			writer.WriteAttributeString("stringfilter", StringFilter);
			writer.WriteAttributeEnum("levelfilter", LevelFilter);
			writer.WriteAttributeBool("colorbylevel", ColorByLevel);
			writer.WriteAttributeInt("selectedentryindex", (int)SelectedLogLine);
			writer.WriteAttributeInt("visibleentryindex", (int)VisibleLogLine);

			writer.WriteStartElement("activatedquickfilters");
			foreach (var guid in ActivatedQuickFilters)
			{
				writer.WriteStartElement("quickfilter");
				writer.WriteAttributeGuid("id", guid);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
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
						IsOpen = reader.ReadContentAsBool();
						break;

					case "followtail":
						FollowTail = reader.ReadContentAsBool();
						break;

					case "stringfilter":
						StringFilter = reader.Value;
						break;

					case "levelfilter":
						LevelFilter = reader.ReadContentAsEnum<LevelFlags>();
						break;

					case "colorbylevel":
						ColorByLevel = reader.ReadContentAsBool();
						break;

					case "selectedentryindex":
						SelectedLogLine = reader.ReadContentAsInt();
						break;

					case "visibleentryindex":
						VisibleLogLine = reader.ReadContentAsInt();
						break;
				}
			}

			reader.MoveToContent();

			var subtree = reader.ReadSubtree();
			while (subtree.Read())
			{
				switch (subtree.Name)
				{
					case "activatedquickfilters":
						var filters = ReadActivatedQuickFilters(reader);
						ActivatedQuickFilters.Clear();
						ActivatedQuickFilters.AddRange(filters);
						break;
				}
			}
		}

		private IEnumerable<Guid> ReadActivatedQuickFilters(XmlReader reader)
		{
			var guids = new List<Guid>();
			var subtree = reader.ReadSubtree();

			while (subtree.Read())
			{
				switch (subtree.Name)
				{
					case "quickfilter":
						int count = reader.AttributeCount;
						for (int i = 0; i < count; ++i)
						{
							reader.MoveToAttribute(i);
							switch (reader.Name)
							{
								case "id":
									guids.Add(reader.ReadContentAsGuid());
									break;
							}
						}
						break;
				}
			}

			return guids;
		}
	}
}