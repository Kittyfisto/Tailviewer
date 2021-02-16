using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Tailviewer.Core.Sources;
using Tailviewer.Settings;

namespace Tailviewer.Ui.Controls.LogView
{
	public abstract class AbstractLogColumnPresenter
		: FrameworkElement
		, ILogFileColumnPresenter
	{
		#region Implementation of ILogFileColumnPresenter

		public abstract IColumnDescriptor Column { get; }
		public abstract TextSettings TextSettings { get; set; }
		public abstract void FetchValues(ILogSource logSource, LogFileSection visibleSection, double yOffset);

		#endregion
	}

	/// <summary>
	///     Responsible for presenting the values of a particular column.
	/// </summary>
	public abstract class AbstractLogColumnPresenter<T>
		: AbstractLogColumnPresenter
	{
		private readonly IColumnDescriptor<T> _column;
		private readonly List<AbstractLogEntryValueFormatter> _values;

		private double _yOffset;
		private TextSettings _textSettings;

		protected AbstractLogColumnPresenter(IColumnDescriptor<T> column, TextSettings textSettings)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));

			_column = column;
			_textSettings = textSettings;
			_values = new List<AbstractLogEntryValueFormatter>();
			ClipToBounds = true;
		}

		protected IEnumerable<AbstractLogEntryValueFormatter> Values => _values;

		public override IColumnDescriptor Column
		{
			get { return _column; }
		}

		public override TextSettings TextSettings
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
		/// <param name="logSource"></param>
		/// <param name="visibleSection"></param>
		/// <param name="yOffset"></param>
		public override void FetchValues(ILogSource logSource, LogFileSection visibleSection, double yOffset)
		{
			if (Visibility != Visibility.Visible) //< We shouldn't waste CPU cycles when we're hidden from view...
				return;

			_yOffset = yOffset;

			_values.Clear();
			if (logSource != null)
			{
				var values = new T[visibleSection.Count];
				logSource.GetColumn(visibleSection, _column, values);
				foreach (var value in values)
					_values.Add(CreateFormatter(value));
			}

			UpdateWidth(logSource, _textSettings);
			InvalidateVisual();
		}

		protected abstract void UpdateWidth(ILogSource logSource, TextSettings textSettings);

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