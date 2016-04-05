using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Tailviewer.BusinessLogic.LogFiles;
using log4net;

namespace Tailviewer.Settings
{
	public sealed class DataSource
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly List<Guid> _activatedQuickFilters;
		public bool ColorByLevel;

		public string File;
		public bool FollowTail;

		/// <summary>
		///     Uniquely identifies this data source amongst all others.
		/// </summary>
		public Guid Id;

		public DateTime LastViewed;
		public LevelFlags LevelFilter;
		public int Order;

		/// <summary>
		///     Uniquely identifies this data-source's parent, if any.
		///     Set to <see cref="Guid.Empty" /> if this data source has no parent.
		/// </summary>
		public Guid ParentId;

		public LogLineIndex SelectedLogLine;
		public string StringFilter;
		public LogLineIndex VisibleLogLine;

		public DataSource()
		{
			Order = -1;
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

		public List<Guid> ActivatedQuickFilters
		{
			get { return _activatedQuickFilters; }
		}

		public override string ToString()
		{
			if (File == null)
				return string.Format("Merged ({0})", Id);

			return string.Format("{0} ({1})", File, Id);
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("file", File);
			writer.WriteAttributeBool("followtail", FollowTail);
			writer.WriteAttributeString("stringfilter", StringFilter);
			writer.WriteAttributeEnum("levelfilter", LevelFilter);
			writer.WriteAttributeBool("colorbylevel", ColorByLevel);
			writer.WriteAttributeInt("selectedentryindex", (int) SelectedLogLine);
			writer.WriteAttributeInt("visibleentryindex", (int) VisibleLogLine);
			writer.WriteAttributeGuid("id", Id);
			writer.WriteAttributeGuid("parentid", ParentId);
			writer.WriteAttributeDateTime("lastviewed", LastViewed);

			writer.WriteStartElement("activatedquickfilters");
			foreach (Guid guid in ActivatedQuickFilters)
			{
				writer.WriteStartElement("quickfilter");
				writer.WriteAttributeGuid("id", guid);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		public void Restore(XmlReader reader, out bool neededPatching)
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

					case "id":
						Id = reader.ReadContentAsGuid();
						break;

					case "parentid":
						ParentId = reader.ReadContentAsGuid();
						break;

					case "lastviewed":
						LastViewed = reader.ReadContentAsDateTime2();
						break;
				}
			}

			if (Id == Guid.Empty)
			{
				Id = Guid.NewGuid();
				Log.InfoFormat("Data Source '{0}' doesn't have an ID yet, setting it to: {1}",
				               File,
				               Id
					);
				neededPatching = true;
			}
			else
			{
				neededPatching = false;
			}

			reader.MoveToContent();

			XmlReader subtree = reader.ReadSubtree();
			while (subtree.Read())
			{
				switch (subtree.Name)
				{
					case "activatedquickfilters":
						IEnumerable<Guid> filters = ReadActivatedQuickFilters(reader);
						ActivatedQuickFilters.Clear();
						ActivatedQuickFilters.AddRange(filters);
						break;
				}
			}
		}

		private IEnumerable<Guid> ReadActivatedQuickFilters(XmlReader reader)
		{
			var guids = new List<Guid>();
			XmlReader subtree = reader.ReadSubtree();

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