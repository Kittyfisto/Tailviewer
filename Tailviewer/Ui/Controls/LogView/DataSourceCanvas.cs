using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
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

		private static readonly int MaximumDataSourceCharacters = 22;

		private readonly List<FormattedText> _dataSourcesPerLogLine;
		private readonly double _maximumWidth;

		private IDataSource _dataSource;
		private LogFileSection _visibleSection;
		private double _yOffset;

		public DataSourceCanvas()
		{
			_dataSourcesPerLogLine = new List<FormattedText>();
			_maximumWidth = TextHelper.EstimateWidthUpperLimit(MaximumDataSourceCharacters);

			ClipToBounds = true;
			SnapsToDevicePixels = true;
		}

		public DataSourceDisplayMode DisplayMode
		{
			get { return (DataSourceDisplayMode) GetValue(DisplayModeProperty); }
			set { SetValue(DisplayModeProperty, value); }
		}

		public IReadOnlyList<FormattedText> DataSources => _dataSourcesPerLogLine;

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

		protected override void OnRender(DrawingContext drawingContext)
		{
			var actualWidth = ActualWidth;
			var actualHeight = ActualHeight;

			drawingContext.DrawRectangle(Brushes.White, pen: null,
				rectangle: new Rect(x: 0, y: 0, width: actualWidth, height: actualHeight));

			var y = _yOffset;
			foreach (var dataSource in _dataSourcesPerLogLine)
			{
				Render(dataSource, drawingContext, y, _maximumWidth);
				y += TextHelper.LineHeight;
			}
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

				var maximumCharacterCount = GetMaximumCharacterCount(displayMode, dataSources);

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

						_dataSourcesPerLogLine.Add(text);
					}
					else
					{
						_dataSourcesPerLogLine.Add(item: null);
					}
				}

				Width = TextHelper.EstimateWidthUpperLimit(maximumCharacterCount);
			}
			else
			{
				_dataSourcesPerLogLine.Clear();
				Width = 0;
			}

			InvalidateVisual();
		}

		/// <summary>
		///     Returns the length of the longest name of any of the given data sources for
		///     that particular display mode.
		/// </summary>
		/// <param name="displayMode"></param>
		/// <param name="dataSources"></param>
		/// <returns></returns>
		private static int GetMaximumCharacterCount(DataSourceDisplayMode displayMode, IReadOnlyList<IDataSource> dataSources)
		{
			var max = 0;
			for (var i = 0; i < dataSources.Count; ++i)
			{
				var name = GetDataSourceName(displayMode, dataSources[i]);
				max = Math.Max(name.Length, max);
			}
			return max;
		}

		/// <summary>
		///     Returns the name of the data source with the given index, depending on the given display mode.
		/// </summary>
		/// <param name="displayMode"></param>
		/// <param name="dataSource"></param>
		/// <returns></returns>
		[Pure]
		private static string GetDataSourceName(DataSourceDisplayMode displayMode, IDataSource dataSource)
		{
			switch (displayMode)
			{
				case DataSourceDisplayMode.CharacterCode:
					return dataSource.CharacterCode;

				case DataSourceDisplayMode.Filename:
					var fullFileName = dataSource.FullFileName;
					var fileName = Path.GetFileName(fullFileName);
					if (fileName != null && fileName.Length > MaximumDataSourceCharacters)
						fileName = fileName.Substring(startIndex: 0, length: MaximumDataSourceCharacters);
					return fileName;

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
			return mergedDataSource;
		}
	}
}