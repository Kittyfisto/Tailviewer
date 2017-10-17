using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using log4net;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.Ui.Controls.LogView
{
	/// <summary>
	///     A "canvas" which draws the data source name in the same vertical alignment as <see cref="TextCanvas" />
	///     draws the <see cref="LogLine.Message" />.
	/// </summary>
	public sealed class DataSourceCanvas
		: FrameworkElement
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly DependencyProperty DisplayModeProperty = DependencyProperty.Register(
			"DisplayMode", typeof(DataSourceDisplayMode), typeof(DataSourceCanvas),
			new PropertyMetadata(DataSourceDisplayMode.Filename, OnDisplayModeChanged));

		private static void OnDisplayModeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((DataSourceCanvas) dependencyObject).OnDisplayModeChanged();
		}

		private void OnDisplayModeChanged()
		{
			// We need to display different text (and our width probably changes as well) so
			// we need to update the list again...
			UpdateDataSources(_dataSource, _visibleSection, _yOffset);
		}

		private readonly List<FormattedText> _dataSourcesPerLogLine;
		private readonly int _maximumDataSourceCharacters;
		private readonly double _maximumWidth;
		private double _yOffset;
		private IDataSource _dataSource;
		private LogFileSection _visibleSection;

		public DataSourceCanvas()
		{
			_dataSourcesPerLogLine = new List<FormattedText>();
			_maximumDataSourceCharacters = 22;
			_maximumWidth = TextHelper.EstimateWidthUpperLimit(_maximumDataSourceCharacters);

			ClipToBounds = true;
		}

		public DataSourceDisplayMode DisplayMode
		{
			get { return (DataSourceDisplayMode) GetValue(DisplayModeProperty); }
			set { SetValue(DisplayModeProperty, value); }
		}

		public IReadOnlyList<FormattedText> DataSources => _dataSourcesPerLogLine;

		protected override void OnRender(DrawingContext drawingContext)
		{
			drawingContext.DrawRectangle(Brushes.White, pen: null,
				rectangle: new Rect(x: 0, y: 0, width: ActualWidth, height: ActualHeight));

			var y = _yOffset;
			foreach (var dataSource in _dataSourcesPerLogLine)
			{
				Render(dataSource, drawingContext, y, _maximumWidth);
				y += TextHelper.LineHeight;
			}
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			e.Handled = true;

			DisplayMode = DisplayMode == DataSourceDisplayMode.CharacterCode
				? DataSourceDisplayMode.Filename
				: DataSourceDisplayMode.CharacterCode;
		}

		public void UpdateDataSources(IDataSource dataSource, LogFileSection visibleSection, double yOffset)
		{
			_dataSource = dataSource;
			_visibleSection = visibleSection;
			_yOffset = yOffset;

			var merged = UnpackMergedDataSource(dataSource);
			if (merged != null)
			{
				var dataSources = merged.OriginalSources;
				var displayMode = DisplayMode;

				int actualCharacterCount = 0;

				_dataSourcesPerLogLine.Clear();
				var logLines = new LogLine[visibleSection.Count];
				merged.UnfilteredLogFile.GetSection(visibleSection, logLines);
				foreach (var logLine in logLines)
				{
					var dataSourceId = (int) logLine.SourceId;
					if (dataSourceId >= 0 && dataSourceId < dataSources.Count)
					{
						var originalDataSource = dataSources[dataSourceId];
						var text = CreateFormattedText(GetDataSourceName(displayMode, originalDataSource));
						if (text != null)
							actualCharacterCount = Math.Max(actualCharacterCount, text.Text.Length);

						_dataSourcesPerLogLine.Add(text);
					}
					else
					{
						_dataSourcesPerLogLine.Add(item: null);
					}
				}

				Width = TextHelper.EstimateWidthUpperLimit(actualCharacterCount);
			}
			else
			{
				_dataSourcesPerLogLine.Clear();
				Width = 0;
			}

			InvalidateVisual();
		}

		/// <summary>
		///     Returns the name of the data source with the given index, depending on the given display mode.
		/// </summary>
		/// <param name="displayMode"></param>
		/// <param name="dataSource"></param>
		/// <returns></returns>
		[Pure]
		private string GetDataSourceName(DataSourceDisplayMode displayMode, IDataSource dataSource)
		{
			switch (displayMode)
			{
				case DataSourceDisplayMode.CharacterCode:
					return dataSource.CharacterCode;

				case DataSourceDisplayMode.Filename:
					var fullFileName = dataSource.FullFileName;
					return Path.GetFileName(fullFileName);

				default:
					return null;
			}
		}

		/// <summary>
		///     Creates an actual formatted text object to render the given data source name.
		/// </summary>
		/// <param name="dataSourceName"></param>
		/// <returns></returns>
		[Pure]
		private FormattedText CreateFormattedText(string dataSourceName)
		{
			// The rendered text may not be greater than N characters...
			try
			{
				if (dataSourceName.Length > _maximumDataSourceCharacters)
					dataSourceName = string.Format("{0}..",
						dataSourceName.Substring(startIndex: 0, length: _maximumDataSourceCharacters - 2));

				var culture = CultureInfo.CurrentUICulture;
				return new FormattedText(dataSourceName,
					culture,
					FlowDirection.LeftToRight,
					TextHelper.Typeface,
					TextHelper.FontSize,
					TextHelper.DataSourceForegroundBrush);
			}
			catch (IOException e)
			{
				// This exception is expected when the path is malformed (for example because that path was passed via command line)
				Log.DebugFormat("Caught io exception: {0}", e);
				return null;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return null;
			}
		}

		private void Render(FormattedText dataSourceName, DrawingContext drawingContext, double y, double dataSourceWidth)
		{
			if (dataSourceName == null)
				return;

			drawingContext.DrawText(dataSourceName, new Point(x: 0, y: y));
		}

		private static IMergedDataSource UnpackMergedDataSource(IDataSource dataSource)
		{
			var mergedDataSource = dataSource as IMergedDataSource;
			if (mergedDataSource != null)
				return mergedDataSource;

			return null;
		}
	}
}