using System.Windows.Media;
using Tailviewer.Settings;

namespace Tailviewer.Themes
{
    /// <summary>
    ///     Provides theme dependent colors and brushes for parts of the UI which are drawn in code
    ///     (e.g. the log view canvas, line numbers, ...) and can therefore not use dynamic XAML resources.
    /// </summary>
    /// <remarks>
    ///     Must be initialized once during application startup via <see cref="Initialize" />.
    /// </remarks>
    public static class AppTheme
    {
        private static Theme _current = Theme.Light;

        /// <summary>
        ///     The theme which is currently applied to the application.
        /// </summary>
        public static Theme Current => _current;

        /// <summary>
        ///     Whether or not the dark theme is currently active.
        /// </summary>
        public static bool IsDark => _current == Theme.Dark;

        /// <summary>
        ///     The background color of the log view.
        /// </summary>
        public static Color LogBackgroundColor => IsDark ? Color.FromRgb(0x1E, 0x1E, 0x1E) : Colors.White;

        /// <summary>
        ///     The background color of every other (alternating) line in the log view.
        /// </summary>
        public static Color LogAlternatingBackgroundColor => IsDark ? Color.FromRgb(0x25, 0x25, 0x26) : Color.FromRgb(0xE8, 0xF1, 0xF7);

        /// <summary>
        ///     The default text color in the log view.
        /// </summary>
        public static Color LogForegroundColor => IsDark ? Color.FromRgb(0xDC, 0xDC, 0xDC) : Colors.Black;

        /// <summary>
        ///     The text color of less important log levels (trace / debug).
        /// </summary>
        public static Color LogDimmedForegroundColor => IsDark ? Color.FromRgb(0x9D, 0x9D, 0x9D) : Color.FromRgb(0x80, 0x80, 0x80);

        /// <summary>
        ///     The color of the line numbers.
        /// </summary>
        public static Color LineNumberForegroundColor => IsDark ? Color.FromRgb(0x5C, 0xB8, 0xD6) : Color.FromRgb(0x2B, 0x91, 0xAF);

        /// <summary>
        ///     The background color of a selected line while the log view does not have keyboard focus.
        /// </summary>
        public static Color SelectedUnfocusedBackgroundColor => IsDark ? Color.FromRgb(0x3F, 0x3F, 0x46) : Color.FromRgb(0xD7, 0xD7, 0xD7);

        /// <summary>
        ///     The background color of the log view.
        /// </summary>
        public static Brush LogBackgroundBrush { get; private set; } = CreateFrozenBrush(Colors.White);

        /// <summary>
        ///     The background brush of scroll bars.
        /// </summary>
        public static Brush ScrollBarBackgroundBrush { get; private set; } = CreateFrozenBrush(Color.FromRgb(0xF0, 0xF0, 0xF0));

        /// <summary>
        ///     The thumb/foreground brush of scroll bars.
        /// </summary>
        public static Brush ScrollBarForegroundBrush { get; private set; } = CreateFrozenBrush(Color.FromRgb(0xC0, 0xC0, 0xC0));

        public static void Initialize(Theme theme)
        {
            _current = theme;
            LogBackgroundBrush = CreateFrozenBrush(LogBackgroundColor);
            ScrollBarBackgroundBrush = CreateFrozenBrush(IsDark ? Color.FromRgb(0x2D, 0x2D, 0x30) : Color.FromRgb(0xF0, 0xF0, 0xF0));
            ScrollBarForegroundBrush = CreateFrozenBrush(IsDark ? Color.FromRgb(0x68, 0x68, 0x6C) : Color.FromRgb(0xC0, 0xC0, 0xC0));
        }

        private static Brush CreateFrozenBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }
}
