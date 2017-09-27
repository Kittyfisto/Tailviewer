using System;
using Tailviewer.Core.Settings;

namespace Tailviewer.QuickInfo.Ui
{
	public sealed class QuickInfoViewConfiguration
		: ICloneable
	{
		public QuickInfoViewConfiguration()
		{
			Name = "New Quick Info";
			Format = "{message}";
		}

		public string Name;

		/// <summary>
		///     An optional format string used to present matches from the filter.
		///     Is only used when <see cref="QuickFilter.MatchType" /> is set to
		///     <see cref="Core.Settings.FilterMatchType.RegexpFilter" />.
		/// </summary>
		/// <remarks>
		///     Shall be in the form of .NET format strings: "v{0}.{1}" uses
		///     the first two matches of the regular expression, etc...
		/// </remarks>
		public string Format;

		public QuickInfoViewConfiguration Clone()
		{
			return new QuickInfoViewConfiguration
			{
				Name = Name,
				Format = Format
			};
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}