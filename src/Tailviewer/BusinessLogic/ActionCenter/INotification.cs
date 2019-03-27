namespace Tailviewer.BusinessLogic.ActionCenter
{
	public interface INotification
	{
		string Title { get; }

		/// <summary>
		///     When set to true then whoever is displaying notifications is told to
		///     make sure that this notification is displayed *immediately*.
		/// </summary>
		bool ForceShow { get; }
	}
}