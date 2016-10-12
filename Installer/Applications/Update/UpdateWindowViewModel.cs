using Metrolib;

namespace Installer.Applications.Update
{
	public sealed class UpdateWindowViewModel
	{
		private readonly UiDispatcher _dispatcher;

		public UpdateWindowViewModel(UiDispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}
	}
}