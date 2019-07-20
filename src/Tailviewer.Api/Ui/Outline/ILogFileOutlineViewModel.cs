using System;
using System.ComponentModel;

namespace Tailviewer.Ui.Outline
{
	/// <summary>
	///     This interface can be implemented to provide a custom outline of a log file to a user.
	///     Used in conjunction with <see cref="ILogFileOutlinePlugin" />.
	/// </summary>
	public interface ILogFileOutlineViewModel
		: INotifyPropertyChanged
		, IDisposable
	{
		/// <summary>
		///     This method is called periodically to allow the view model to update itself.
		/// </summary>
		/// <remarks>
		///     This method is ALWAYS called on the UI thread which means that this method should
		///     execute as quick as possible and not perform any I/O!
		/// </remarks>
		void Update();
	}
}