using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using log4net;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Ui.Controls.LogView
{
	/// <summary>
	///     A "canvas" which draws the data source name in the same vertical alignment as <see cref="TextCanvas" />
	///     draws the <see cref="LogLine.Message" />.
	/// </summary>
	public sealed  class DataSourceCanvas
		: FrameworkElement
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly List<FormattedText> _dataSourcesPerLogLine;
		private readonly int _maximumDataSourceCharacters;
		private readonly double _maximumWidth;
		private double _yOffset;

		public DataSourceCanvas()
		{
			_dataSourcesPerLogLine = new List<FormattedText>();
			_maximumDataSourceCharacters = 22;
			_maximumWidth = TextHelper.EstimateWidthUpperLimit(_maximumDataSourceCharacters);

			ClipToBounds = true;
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

		public void UpdateLineNumbers(IDataSource dataSource, LogFileSection visibleSection, double yOffset)
		{
			var merged = UnpackMergedDataSource(dataSource);
			if (merged != null)
			{
				var dataSources = merged.OriginalSources;
				_yOffset = yOffset;
				Width = _maximumWidth;

				_dataSourcesPerLogLine.Clear();
				var logLines = new LogLine[visibleSection.Count];
				// TODO: Is unfiltered correct here?!?
				merged.UnfilteredLogFile.GetSection(visibleSection, logLines);
				foreach (var logLine in logLines)
				{
					var dataSourceId = (int)logLine.SourceId;
					if (dataSourceId >= 0 && dataSourceId < dataSources.Count)
					{
						_dataSourcesPerLogLine.Add(CreateDataSourceName(dataSources[dataSourceId]));
					}
					else
					{
						// This will happen because:
						// 1) Access to the log file is not synchronized and we're trying to draw
						//    an old state which the log file no longe represents. This will self
						//    correct because a subsequent draw will match the log file's state...
						//
						// 2) A programming error (well, at least the application isn't crashing).
						_dataSourcesPerLogLine.Add(null);
					}
				}
			}
			else
			{
				_dataSourcesPerLogLine.Clear();
				Width = 0;
			}

			InvalidateVisual();
		}

		private FormattedText CreateDataSourceName(IDataSource dataSource)
		{
			// The rendered text may not be greater than N characters...
			var fullPath = dataSource.FullFileName;
			try
			{
				var fileName = Path.GetFileName(fullPath);
				if (fileName == null)
					return null;

				if (fileName.Length > _maximumDataSourceCharacters)
				{
					fileName = string.Format("{0}...", fileName.Substring(0, _maximumDataSourceCharacters - 3));
				}

				var culture = CultureInfo.CurrentUICulture;
				return new FormattedText(fileName,
					culture,
					FlowDirection.LeftToRight,
					TextHelper.Typeface,
					TextHelper.FontSize,
					TextHelper.LineNumberForegroundBrush);
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

			drawingContext.DrawText(dataSourceName, new Point(0, y));
		}

		private static IMergedDataSource UnpackMergedDataSource(IDataSource dataSource)
		{
			var mergedDataSource = dataSource as IMergedDataSource;
			if (mergedDataSource != null)
			{
				return mergedDataSource;
			}

			return null;
		}
	}
}