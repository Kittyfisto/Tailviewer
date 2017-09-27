using System;
using System.Diagnostics.Contracts;
using Tailviewer.Core.Settings;

namespace Tailviewer.QuickInfo.BusinessLogic
{
	/// <summary>
	///     The configuration of a single quick info.
	/// </summary>
	public sealed class QuickInfoConfiguration
		: ICloneable
	{
		public QuickInfoConfiguration()
		{
			MatchType = FilterMatchType.RegexpFilter;
		}

		/// <summary>
		///     The filter used to find matching lines.
		/// </summary>
		public string FilterValue;

		public FilterMatchType MatchType;

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
	}
}