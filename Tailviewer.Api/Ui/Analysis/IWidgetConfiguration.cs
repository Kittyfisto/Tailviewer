using System;

namespace Tailviewer.Ui.Analysis
{
	/// <summary>
	///     The interface for a configuration that affects the way a widget presents its data
	///     (but not the analysis).
	/// </summary>
	public interface IWidgetConfiguration
		: ISerializableType
		, ICloneable
	{
	}
}