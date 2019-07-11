namespace Tailviewer.Archiver.Plugins.Description
{
	public interface IChange
	{
		/// <summary>
		///     A required short one sentence summary of the change.
		/// </summary>
		string Summary { get; }

		/// <summary>
		///     An optional (possibly longer) description of the change.
		/// </summary>
		string Description { get; }
	}
}