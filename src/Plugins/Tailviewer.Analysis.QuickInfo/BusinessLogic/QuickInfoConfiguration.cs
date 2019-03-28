using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using Tailviewer.Core.Settings;

namespace Tailviewer.Analysis.QuickInfo.BusinessLogic
{
	/// <summary>
	///     The configuration of a single quick info.
	/// </summary>
	[DataContract]
	public sealed class QuickInfoConfiguration
		: ISerializableType
		, ICloneable
	{
		public QuickInfoConfiguration()
		{
			MatchType = FilterMatchType.RegexpFilter;
		}

		public QuickInfoConfiguration(Guid id)
			: this()
		{
			_id = id;
		}

		public Guid Id => _id;

		/// <summary>
		///     The filter used to find matching lines.
		/// </summary>
		public string FilterValue;

		public FilterMatchType MatchType;
		private Guid _id;

		object ICloneable.Clone()
		{
			return Clone();
		}

		[Pure]
		public QuickInfoConfiguration Clone()
		{
			return new QuickInfoConfiguration
			{
				FilterValue = FilterValue,
				MatchType = MatchType
			};
		}

		#region Implementation of ISerializableType

		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Id", _id);
			writer.WriteAttribute("FilterValue", FilterValue);
			writer.WriteAttribute("MatchType", MatchType.ToString());
		}

		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Id", out _id);
			reader.TryReadAttribute("FilterValue", out FilterValue);
			if (reader.TryReadAttribute("MatchType", out string matchType))
				Enum.TryParse(matchType, out MatchType);
		}

		#endregion
	}
}