using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Tailviewer.BusinessLogic.LogFiles;

namespace LogViewer
{
	public sealed class TextLine
	{
		public const double FontSize = 12;
		public const double LineSpacing = 4;
		public const double LineHeight = FontSize + LineSpacing;
		private static readonly Brush NormalForegroundBrush;
		private static readonly Brush NormalBackgroundBrush;
		private static readonly Brush HoveredForegroundBrush;
		private static readonly Brush HoveredBackgroundBrush;
		private static readonly Brush SelectedForegroundBrush;
		private static readonly Brush SelectedBackgroundBrush;
		private static readonly Typeface Typeface;

		private readonly LogLine _logEntry;
		private bool _isHovered;
		private bool _isSelected;
		private FormattedText _text;

		static TextLine()
		{
			NormalBackgroundBrush = null;
			NormalForegroundBrush = Brushes.Black;
			HoveredBackgroundBrush = new SolidColorBrush(Color.FromRgb(242, 242, 242));
			HoveredForegroundBrush = Brushes.Black;
			SelectedBackgroundBrush = new SolidColorBrush(Color.FromRgb(57, 152, 214));
			SelectedForegroundBrush = Brushes.White;
			Typeface = new Typeface("Segoe UI");
		}

		public TextLine(LogLine logEntry)
		{
			_logEntry = logEntry;
		}

		public bool IsHovered
		{
			get { return _isHovered; }
			set
			{
				if (value == _isHovered)
					return;

				_isHovered = value;
				_text = null;
			}
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (value == _isSelected)
					return;

				_isSelected = value;
				_text = null;
			}
		}

		public FormattedText Text
		{
			get
			{
				CreateTextIfNecessary();
				return _text;
			}
		}

		private Brush ForegroundBrush
		{
			get
			{
				if (_isSelected)
				{
					return SelectedForegroundBrush;
				}
				if (_isHovered)
				{
					return HoveredForegroundBrush;
				}

				return NormalForegroundBrush;
			}
		}

		private Brush BackgroundBrush
		{
			get
			{
				if (_isSelected)
				{
					return SelectedBackgroundBrush;
				}
				if (_isHovered)
				{
					return HoveredBackgroundBrush;
				}
				return NormalBackgroundBrush;
			}
		}

		private void CreateTextIfNecessary()
		{
			if (_text != null)
				return;

			Brush brush = ForegroundBrush;
			_text = new FormattedText(_logEntry.Message,
			                          CultureInfo.CurrentUICulture,
			                          FlowDirection.LeftToRight,
			                          Typeface,
			                          FontSize,
			                          brush);
		}

		public void Render(DrawingContext drawingContext, double y, double actualWidth)
		{
			Brush brush = BackgroundBrush;
			if (brush != null)
			{
				drawingContext.DrawRectangle(brush, null, new Rect(0, y, actualWidth, LineHeight));
			}

			var topLeft = new Point(0, y);
			drawingContext.DrawText(Text, topLeft);
		}
	}
}