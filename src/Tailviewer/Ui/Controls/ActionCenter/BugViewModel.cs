using Tailviewer.BusinessLogic.ActionCenter;

namespace Tailviewer.Ui.Controls.ActionCenter
{
	public sealed class BugViewModel
		: AbstractNotificationViewModel
	{
		private readonly IBug _bug;

		public BugViewModel(IBug bug) : base(bug)
		{
			_bug = bug;
		}

		public string Details
		{
			get
			{
				return _bug.Details;
			}
		}
	}
}