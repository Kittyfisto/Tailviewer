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
		public QuickInfoConfiguration()
		{
			Filter = new QuickFilter();
		}

		/// <summary>
		///     The filter used to find matching lines.
		/// </summary>
		public QuickFilter Filter;

		object ICloneable.Clone()
		{
			return Clone();
		}

		[Pure]
		public QuickInfoConfiguration Clone()
		{
			return new QuickInfoConfiguration
			{
				Filter = Filter.Clone(),
			};
		}
	}
}