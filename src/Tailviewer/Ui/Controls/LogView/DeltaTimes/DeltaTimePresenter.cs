using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Tailviewer.Ui.Controls.LogView.DeltaTimes
{
	internal sealed class DeltaTimePresenter
		: AbstractLogEntryValuePresenter
	{
		private readonly TimeSpan? _delta;

		public DeltaTimePresenter(TimeSpan? delta)
		{
			_delta = delta;
		}

		public override string ToString(IFormatProvider provider)
		{
			return Format(_delta, provider);
		}

		public static string Format(TimeSpan? value, IFormatProvider provider)
		{
			if (value != null)
			{
				var delta = value.Value;
				var absDelta = TimeSpan.FromMilliseconds(Math.Abs(delta.TotalMilliseconds));
				if (absDelta >= TimeSpan.FromDays(value: 1))
					return ToString(provider, (int) delta.TotalDays, "day", "days");
				if (absDelta >= TimeSpan.FromHours(value: 1))
					return ToString(provider, (int) delta.TotalHours, "hour", "hours");
				if (absDelta >= TimeSpan.FromMinutes(value: 1))
					return ToString(provider, (int) delta.TotalMinutes, "min");
				if (absDelta >= TimeSpan.FromSeconds(value: 1))
					return ToString(provider, (int) delta.TotalSeconds, "sec");

				return ToString(provider, (int) delta.TotalMilliseconds, "ms");
			}

			return string.Empty;
		}

		protected override FormattedText CreateFormattedText(string text, CultureInfo culture)
		{
			return new FormattedText(text,
			                         culture,
			                         FlowDirection.LeftToRight,
			                         TextHelper.Typeface,
			                         TextHelper.FontSize,
			                         TextHelper.LineNumberForegroundBrush,
			                         1.25);
		}

		private static string ToString(IFormatProvider provider, int rawValue, string singularSuffix, string pluralSuffix)
		{
			var suffix = GetSuffix(rawValue, singularSuffix, pluralSuffix);
			return ToString(provider, rawValue, suffix);
		}

		private static string ToString(IFormatProvider provider, int rawValue, string suffix)
		{
			var sign = GetSign(rawValue);
			return string.Format(provider, "{0}{1} {2}", sign, rawValue, suffix);
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
	}
}