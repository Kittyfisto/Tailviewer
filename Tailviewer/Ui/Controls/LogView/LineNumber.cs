using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Ui.Controls.LogView
{
	public struct LineNumber
	{
		private readonly int _number;
		private FormattedText _text;

		public LineNumber(LogLineIndex logLineIndex)
		{
			_number = (int)(logLineIndex + 1);
			_text = null;
		}

		public override string ToString()
		{
			return ToString(CultureInfo.CurrentUICulture);
		}

		private FormattedText Text
		{
			get
			{
				if (_text == null)
				{
					var culture = CultureInfo.CurrentUICulture;
					_text = new FormattedText(ToString(culture),
					                          culture,
					                          FlowDirection.LeftToRight,
					                          TextHelper.Typeface,
											  TextHelper.FontSize,
											  TextHelper.LineNumberForegroundBrush);
				}
				return _text;
			}
		}

		private string ToString(CultureInfo culture)
		{
			return _number.ToString(culture);
		}

		public void Render(DrawingContext drawingContext, double yOffset, double lineNumberWidth)
		{
			var text = Text;

			// We want to place the line numbers right aligned.
			// Although _text.Width is an option, it is INCREDIBLY slow.
			// It works by creating a polygon that represents the text and
			// then calculating the MBR of said polygon, which is fubar.
			var width = TextHelper.EstimateWidthUpperLimit(text.Text);
			var x = lineNumberWidth - width;

			drawingContext.DrawText(text, new Point(x, yOffset));
		}
	}
}