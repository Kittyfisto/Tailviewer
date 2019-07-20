using System;

namespace Tailviewer.BusinessLogic.Plugins
{
	/// <summary>
	///     This interface can be implemented to provide a custom outline of a log file to a user.
	///     Used in conjunction with <see cref="ILogFileOutlinePlugin" />.
	/// </summary>
	public interface ILogFileOutlineViewModel
		: IDisposable
	{
	}
}