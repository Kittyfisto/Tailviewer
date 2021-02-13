using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.Ui.Controls.LogView.Any
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class AnyColumnPresenter<T>
		: AbstractLogColumnPresenter<T>
	{
		public AnyColumnPresenter(ILogFileColumnDescriptor<T> column, TextSettings textSettings)
			: base(column, textSettings)
		{
		}

		#region Overrides of AbstractLogColumnPresenter<T>

		protected override void UpdateWidth(ILogFile logFile, TextSettings textSettings)
		{}

		protected override AbstractLogEntryValueFormatter CreateFormatter(T value)
		{
			return new AnyFormatter<T>(value, TextSettings);
		}

		#endregion
	}
}
