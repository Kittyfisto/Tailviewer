using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.Ui.Controls.LogView
{
	/// <summary>
	///     Responsible for presenting the values of a particular column.
	/// </summary>
	public abstract class AbstractLogColumnPresenter<T>
		: FrameworkElement
	{
		private readonly ILogFileColumn<T> _column;
		private readonly List<AbstractLogEntryValueFormatter> _values;

		private double _yOffset;
		private TextSettings _textSettings;

		protected AbstractLogColumnPresenter(ILogFileColumn<T> column, TextSettings textSettings)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			_column = column;
			_textSettings = textSettings;
			_values = new List<AbstractLogEntryValueFormatter>();
			ClipToBounds = true;
		}

		protected IEnumerable<AbstractLogEntryValueFormatter> Values => _values;

		public TextSettings TextSettings
		{
			get { return _textSettings; }
			set
			{
				_textSettings = value;
				// TODO:
			}
		}

		/// <summary>
		///     Fetches the newest values for this presenter's column from the given log file.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="visibleSection"></param>
		/// <param name="yOffset"></param>
		public void FetchValues(ILogFile logFile, LogFileSection visibleSection, double yOffset)
		{
			if (Visibility != Visibility.Visible) //< We shouldn't waste CPU cycles when we're hidden from view...
				return;

			_yOffset = yOffset;

			_values.Clear();
			if (logFile != null)
			{
				var values = new T[visibleSection.Count];
				logFile.GetColumn(visibleSection, _column, values);
				foreach (var value in values)
					_values.Add(CreateFormatter(value));
			}

			UpdateWidth(logFile, _textSettings);
			InvalidateVisual();
		}

		protected abstract void UpdateWidth(ILogFile logFile, TextSettings textSettings);

		/// <summary>
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected abstract AbstractLogEntryValueFormatter CreateFormatter(T value);

		protected override void OnRender(DrawingContext drawingContext)
		{
			drawingContext.DrawRectangle(Brushes.White, pen: null,
			                             rectangle: new Rect(x: 0, y: 0, width: ActualWidth, height: ActualHeight));

			var y = _yOffset;
			foreach (var number in _values)
			{
				number.Render(drawingContext, y, Width);
				y += _textSettings.LineHeight;
			}
		}
	}
}