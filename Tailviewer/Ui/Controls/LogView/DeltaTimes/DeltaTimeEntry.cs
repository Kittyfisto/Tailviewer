using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Tailviewer.Ui.Controls.LogView.DeltaTimes
{
	internal struct DeltaTimeEntry
	{
		private readonly TimeSpan? _delta;
		private FormattedText _formattedText;

		public DeltaTimeEntry(TimeSpan? delta)
		{
			_delta = delta;
			_formattedText = null;
		}

		public override string ToString()
		{
			return ToString(CultureInfo.InvariantCulture);
		}

		public string ToString(IFormatProvider provider)
		{
			if (_delta != null)
			{
				var delta = _delta.Value;
				var absDelta = TimeSpan.FromMilliseconds(Math.Abs(delta.TotalMilliseconds));
				if (absDelta >= TimeSpan.FromDays(1))
					return ToString(provider, (int)delta.TotalDays, "day", "days");
				if (absDelta >= TimeSpan.FromHours(1))
					return ToString(provider, (int)delta.TotalHours, "hour", "hours");
				if (absDelta >= TimeSpan.FromMinutes(1))
					return ToString(provider, (int)delta.TotalMinutes, "min");
				if (absDelta >= TimeSpan.FromSeconds(1))
					return ToString(provider, (int)delta.TotalSeconds, "sec");

				return ToString(provider, (int)delta.TotalMilliseconds, "ms");
			}

			return string.Empty;
		}

		private static string ToString(IFormatProvider provider, int rawValue, string singularSuffix, string pluralSuffix)
		{
			var suffix = GetSuffix(rawValue, singularSuffix, pluralSuffix);
			return ToString(provider, rawValue, suffix);
		}

		private static string ToString(IFormatProvider provider, int rawValue, string suffix)
		{
			var sign = GetSign(rawValue);
			return string.Format(provider, "{0}{1}{2}", sign, rawValue, suffix);
		}

		[Pure]
		private static string GetSuffix(int rawValue, string singularSuffix, string pluralSuffix)
		{
			if (rawValue == -1 || rawValue == 1)
				return singularSuffix;

			return pluralSuffix;
		}

		[Pure]
		private static string GetSign(int rawValue)
		{
			if (rawValue > 0)
				return "+";

			return null;
		}

		private FormattedText FormattedText
		{
			get
			{
				if (_formattedText == null)
				{
					var culture = CultureInfo.CurrentUICulture;
					var text = ToString(culture);
					_formattedText = new FormattedText(text,
					                                   culture,
					                                   FlowDirection.LeftToRight,
					                                   TextHelper.Typeface,
					                                   TextHelper.FontSize,
					                                   TextHelper.LineNumberForegroundBrush);
				}
				return _formattedText;
			}
		}

		public void Render(DrawingContext drawingContext, double yOffset, double lineNumberWidth)
		{
			var text = FormattedText;

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