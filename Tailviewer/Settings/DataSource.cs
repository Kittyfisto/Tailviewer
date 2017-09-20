using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Tailviewer.BusinessLogic;
using log4net;
using Metrolib;
using Tailviewer.Core;

namespace Tailviewer.Settings
{
	public sealed class DataSource
		: ICloneable
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly List<QuickFilterId> _activatedQuickFilters;
		private readonly HashSet<AnalysisId> _analyses;

		public bool ColorByLevel;
		public bool HideEmptyLines;
		public bool IsSingleLine;

		public string File;
		public bool FollowTail;
		public bool ShowLineNumbers;
		public bool IsExpanded;

		/// <summary>
		///     Uniquely identifies this data source amongst all others.
		/// </summary>
		public DataSourceId Id;

		public DateTime LastViewed;
		public LevelFlags LevelFilter;
		public int Order;

		/// <summary>
		///     Uniquely identifies this data-source's parent, if any.
		///     Set to <see cref="Guid.Empty" /> if this data source has no parent.
		/// </summary>
		public DataSourceId ParentId;

		public HashSet<LogLineIndex> SelectedLogLines;
		public string SearchTerm;
		public LogLineIndex VisibleLogLine;
		public double HorizontalOffset;

		public DataSource()
		{
			Order = -1;

			_activatedQuickFilters = new List<QuickFilterId>();
			_analyses = new HashSet<AnalysisId>();

			LevelFilter = LevelFlags.All;
			ColorByLevel = true;
			ShowLineNumbers = true;
			IsExpanded = true;
			SelectedLogLines = new HashSet<LogLineIndex>();
			VisibleLogLine = LogLineIndex.Invalid;
		}

		public DataSource(string file)
			: this()
		{
			File = file;
		}

		public List<QuickFilterId> ActivatedQuickFilters => _activatedQuickFilters;

		public HashSet<AnalysisId> Analyses => _analyses;

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
			writer.WriteAttributeBool("showlinenumbers", ShowLineNumbers);
			writer.WriteAttributeString("searchterm", SearchTerm);
			writer.WriteAttributeEnum("levelfilter", LevelFilter);
			writer.WriteAttributeBool("colorbylevel", ColorByLevel);
			writer.WriteAttributeBool("hideemptylines", HideEmptyLines);
			writer.WriteAttributeBool("singleline", IsSingleLine);
			writer.WriteAttributeBool("expanded", IsExpanded);
			writer.WriteAttributeInt("visibleentryindex", (int) VisibleLogLine);
			writer.WriteAttribute("id", Id);
			writer.WriteAttribute("parentid", ParentId);
			writer.WriteAttributeDateTime("lastviewed", LastViewed);
			writer.WriteAttributeDouble("horizontaloffset", HorizontalOffset);

			writer.WriteStartElement("activatedquickfilters");
			foreach (QuickFilterId guid in ActivatedQuickFilters)
			{
				writer.WriteStartElement("quickfilter");
				writer.WriteAttribute("id", guid);
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

					case "showlinenumbers":
						ShowLineNumbers = reader.ReadContentAsBool();
						break;

					case "stringfilter":
					case "searchterm":
						SearchTerm = reader.Value;
						break;

					case "levelfilter":
						LevelFilter = reader.ReadContentAsEnum<LevelFlags>();
						break;

					case "colorbylevel":
						ColorByLevel = reader.ReadContentAsBool();
						break;

					case "hideemptylines":
						HideEmptyLines = reader.ReadContentAsBool();
						break;

					case "singleline":
						IsSingleLine = reader.ReadContentAsBool();
						break;

					case "expanded":
						IsExpanded = reader.ReadContentAsBool();
						break;

					case "visibleentryindex":
						VisibleLogLine = reader.ReadContentAsInt();
						break;

					case "id":
						Id = reader.ReadContentAsDataSourceId();
						break;

					case "parentid":
						ParentId = reader.ReadContentAsDataSourceId();
						break;

					case "lastviewed":
						LastViewed = reader.ReadContentAsDateTime2();
						break;

					case "horizontaloffset":
						HorizontalOffset = reader.ReadContentAsDouble2();
						break;
				}
			}

			if (Id == DataSourceId.Empty)
			{
				Id = DataSourceId.CreateNew();
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
						IEnumerable<QuickFilterId> filters = ReadActivatedQuickFilters(reader);
						ActivatedQuickFilters.Clear();
						ActivatedQuickFilters.AddRange(filters);
						break;
				}
			}
		}

		private IEnumerable<QuickFilterId> ReadActivatedQuickFilters(XmlReader reader)
		{
			var guids = new List<QuickFilterId>();
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
									guids.Add(reader.ReadContentAsQuickFilterId());
									break;
							}
						}
						break;
				}
			}

			return guids;
		}

		public DataSource Clone()
		{
			var clone = new DataSource
			{
				ColorByLevel = ColorByLevel,
				File = File,
				FollowTail = FollowTail,
				HideEmptyLines = HideEmptyLines,
				HorizontalOffset = HorizontalOffset,
				Id = Id,
				IsSingleLine = IsSingleLine,
				LastViewed = LastViewed,
				LevelFilter = LevelFilter,
				Order = Order,
				ParentId = ParentId,
				SearchTerm = SearchTerm,
				ShowLineNumbers = ShowLineNumbers,
				VisibleLogLine = VisibleLogLine,
				IsExpanded = IsExpanded
			};
			clone.ActivatedQuickFilters.AddRange(ActivatedQuickFilters);
			clone.SelectedLogLines.AddRange(SelectedLogLines);
			return clone;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}