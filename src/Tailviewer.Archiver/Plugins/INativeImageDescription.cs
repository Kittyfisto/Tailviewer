namespace Tailviewer.Archiver.Plugins
{
	public interface INativeImageDescription
	{
		/// <summary>
		///     The file name of the native image in the package.
		/// </summary>
		string EntryName { get; }

		/// <summary>
		///     The name of the native image, excluding its file extension.
		/// </summary>
		string ImageName { get; }
	}
}