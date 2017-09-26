using System;
using System.Diagnostics.Contracts;
using Tailviewer.Core.Settings;

namespace Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo
{
	/// <summary>
	///     The configuration of a single quick info.
	/// </summary>
	public sealed class QuickInfoConfiguration
		: ICloneable
	{
		/// <summary>
		///     The filter used to find matching lines.
		/// </summary>
		public QuickFilter Filter;

		/// <summary>
		///     An optional format string used to present matches from the filter.
		///     Is only used when <see cref="QuickFilter.MatchType" /> is set to
		///     <see cref="QuickFilterMatchType.RegexpFilter" />.
		/// </summary>
		/// <remarks>
		///     Shall be in the form of .NET format strings: "v{0}.{1}" uses
		///     the first two matches of the regular expression, etc...
		/// </remarks>
		public string Format;

		/// <summary>
		///     Identifier for this quick info.
		/// </summary>
		public Guid Id;

		object ICloneable.Clone()
		{
			return Clone();
		}

		[Pure]
		public QuickInfoConfiguration Clone()
		{
			return new QuickInfoConfiguration
			{
				Id = Id,
				Filter = Filter.Clone(),
				Format = Format
			};
		}
	}
}