namespace Tailviewer.Core.Analysis
{
	/// <summary>
	///     The template for a horizontal widget layout:
	///     Used to persist the settings of the layout in between sessions.
	/// </summary>
	public sealed class HorizontalWidgetLayoutTemplate
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
	}
}