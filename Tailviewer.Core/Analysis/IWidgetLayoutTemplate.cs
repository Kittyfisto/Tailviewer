using System;

namespace Tailviewer.Core.Analysis
{
	/// <summary>
	///     The interface for the template of a widget layout.
	///     Is used to persist layout-specific information in between
	///     sessions.
	/// </summary>
	public interface IWidgetLayoutTemplate
		: ISerializableType
		, ICloneable
	{
	}
}