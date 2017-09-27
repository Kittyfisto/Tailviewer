using System;
using System.Diagnostics.Contracts;
using System.Xml;
using Metrolib;
using Tailviewer.Core.Filters;

namespace Tailviewer.Core.Settings
{
	public sealed class QuickFilter
		: ICloneable
	{
		public bool IgnoreCase;
		public bool IsInverted;
		public FilterMatchType MatchType;
		public string Value;

		public QuickFilter()
		{
			Id = QuickFilterId.CreateNew();
			IgnoreCase = true;
			IsInverted = false;
		}

		public QuickFilterId Id { get; private set; }

		object ICloneable.Clone()
		{
			return Clone();
		}

		public bool Restore(XmlReader reader)
		{
			var count = reader.AttributeCount;
			for (var i = 0; i < count; ++i)
			{
				reader.MoveToAttribute(i);

				switch (reader.Name)
				{
					case "id":
						Id = reader.ReadContentAsQuickFilterId();
						break;

					case "type":
						MatchType = reader.ReadContentAsEnum<FilterMatchType>();
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
				Id = Id,
				IgnoreCase = IgnoreCase,
				IsInverted = IsInverted,
				MatchType = MatchType,
				Value = Value
			};
		}

		/// <summary>
		///     Tests if this filter and the given one would produce the same result
		///     for the same data.
		/// </summary>
		/// <param name="other"></param>
		/// <returns>True when there is no doubt that the two filters perform identical, false otherwise</returns>
		public bool IsEquivalent(QuickFilter other)
		{
			if (ReferenceEquals(other, objB: null))
				return false;

			// We won't need to include the id because it doesn't have
			// any influence on the outcome of a filter operation.

			if (IgnoreCase != other.IgnoreCase)
				return false;

			if (IsInverted != other.IsInverted)
				return false;

			if (MatchType != other.MatchType)
				return false;

			if (!Equals(Value, other.Value))
				return false;

			return true;
		}

		[Pure]
		public ILogEntryFilter CreateFilter()
		{
			return Filter.Create(Value, MatchType, IgnoreCase, IsInverted);
		}
	}
}