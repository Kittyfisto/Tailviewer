using System;
using System.Xml;
using Metrolib;

namespace Tailviewer.Core.Settings
{
	public sealed class QuickFilter
		: ICloneable
	{
		public bool IgnoreCase;
		public QuickFilterMatchType MatchType;
		public string Value;
		public bool IsInverted;
		private QuickFilterId _id;

		public QuickFilter()
		{
			_id = QuickFilterId.CreateNew();
			IgnoreCase = true;
			IsInverted = false;
		}

		public QuickFilterId Id => _id;

		public bool Restore(XmlReader reader)
		{
			int count = reader.AttributeCount;
			for (int i = 0; i < count; ++i)
			{
				reader.MoveToAttribute(i);

				switch (reader.Name)
				{
					case "id":
						_id = reader.ReadContentAsQuickFilterId();
						break;

					case "type":
						MatchType = reader.ReadContentAsEnum<QuickFilterMatchType>();
						break;

					case "value":
						Value = reader.Value;
						break;

					case "ignorecase":
						IgnoreCase = reader.ReadContentAsBool();
						break;

					case "isinclude":
						IsInverted = reader.ReadContentAsBool();
						break;
				}
			}

			if (Id == QuickFilterId.Empty)
				return false;

			return true;
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttribute("id", Id);
			writer.WriteAttributeEnum("type", MatchType);
			writer.WriteAttributeString("value", Value);
			writer.WriteAttributeBool("ignorecase", IgnoreCase);
			writer.WriteAttributeBool("isinclude", IsInverted);
		}

		public QuickFilter Clone()
		{
			return new QuickFilter
			{
				_id = _id,
				IgnoreCase = IgnoreCase,
				IsInverted = IsInverted,
				MatchType = MatchType,
				Value = Value
			};
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}