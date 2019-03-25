using System;
using System.Diagnostics.Contracts;

namespace Tailviewer.Core.Analysis.Layouts
{
	/// <summary>
	///     The template for a row widget layout:
	///     Used to persist the settings of the layout in between sessions.
	/// </summary>
	public sealed class RowWidgetLayoutTemplate
		: IWidgetLayoutTemplate
	{
		/// <inheritdoc />
		public void Serialize(IWriter writer)
		{
		}

		/// <inheritdoc />
		public void Deserialize(IReader reader)
		{
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		///     Creates a deep clone of this template.
		/// </summary>
		/// <returns></returns>
		[Pure]
		public RowWidgetLayoutTemplate Clone()
		{
			return new RowWidgetLayoutTemplate();
		}

		/// <inheritdoc />
		public PageLayout PageLayout => PageLayout.Rows;
	}
}