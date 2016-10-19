namespace Tailviewer.BusinessLogic.ActionCenter
{
	public interface IBug
		: INotification
	{
		string Details { get; }
	}
}