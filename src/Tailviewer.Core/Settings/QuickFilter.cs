using System;
using System.Diagnostics.Contracts;
using System.Xml;
using Metrolib;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.Core.Filters;

namespace Tailviewer.Core.Settings
{
	/// <summary>
	///     The configuration of an application-wide quick filter.
	/// </summary>
	public sealed class QuickFilter
		: ICloneable
		, ISerializableType
	{
		/// <summary>
		///     The id of this quick filter.
		///     Is used to define for each data source which quick filter is active or not.
		/// </summary>
		public QuickFilterId Id;

		/// <summary>
		///     True when the case of the filter value doesn't matter.
		/// </summary>
		public bool IgnoreCase;

		/// <summary>
		///     When set to false, then a line will only be shown if it matches the filter.
		///     When set to true, then only those lines NOT matching the filter will be shown.
		/// </summary>
		public bool IsInverted;

		/// <summary>
		///     How <see cref="Value" /> is to be intepreted.
		/// </summary>
		public FilterMatchType MatchType;

		/// <summary>
		///     The actual filter value, <see cref="MatchType" /> defines how it is interpreted.
		/// </summary>
		public string Value;

		/// <summary>
		///     Initializes this quick filter.
		/// </summary>
		public QuickFilter()
		{
			Id = QuickFilterId.CreateNew();
			IgnoreCase = true;
			IsInverted = false;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		///     Restores this filter from the given xml reader.
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
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

		/// <summary>
		///     Saves the contents of this object into the given writer.
		/// </summary>
		/// <param name="writer"></param>
		public void Save(XmlWriter writer)
		{
			writer.WriteAttribute("id", Id);
			writer.WriteAttributeEnum("type", MatchType);
			writer.WriteAttributeString("value", Value);
			writer.WriteAttributeBool("ignorecase", IgnoreCase);
			writer.WriteAttributeBool("isinclude", IsInverted);
		}

		/// <summary>
		///     Creates a deep clone of this object.
		/// </summary>
		/// <returns></returns>
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

		/// <summary>
		///     Creates a new <see cref="ILogEntryFilter" /> which behaves just like this quick filter is
		///     configured. Any further changes to this object will NOT influence the returned filter:
		///     This method must be called and the newly returned filter be used instead.
		/// </summary>
		/// <returns></returns>
		[Pure]
		public ILogEntryFilter CreateFilter()
		{
			return Filter.Create(Value, MatchType, IgnoreCase, IsInverted);
		}

		/// <inheritdoc />
		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Id", Id);
			writer.WriteAttributeEnum("Type", MatchType);
			writer.WriteAttribute("Value", Value);
			writer.WriteAttribute("IgnoreCase", IgnoreCase);
			writer.WriteAttribute("IsInverted", IsInverted);
		}

		/// <inheritdoc />
		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Id", out Id);
			reader.TryReadAttributeEnum("Type", out MatchType);
			reader.TryReadAttribute("Value", out Value);
			reader.TryReadAttribute("IgnoreCase", out IgnoreCase);
			reader.TryReadAttribute("IsInverted", out IsInverted);
		}
	}
}