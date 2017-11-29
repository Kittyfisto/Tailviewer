using System.Windows;
using System.Windows.Media;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Ui.Controls.LogView
{
	/// <summary>
	///     A "canvas" which draws the elapsed time to the previous log entry in the same vertical alignment as <see cref="TextCanvas" />
	///     draws the <see cref="LogLine.Message" />.
	/// </summary>
	public sealed class LogEntryElapsedCanvas
		: FrameworkElement
	{
		public LogEntryElapsedCanvas()
		{
			
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			
		}

		public void UpdateLines(ILogFile logFile, LogFileSection currentlyVisibleSection, double yOffset)
		{
			
		}
	}
}