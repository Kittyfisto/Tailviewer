using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
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

		private readonly List<string> _characterCodesPerDataSource;

		private readonly List<FormattedText> _dataSourcesPerLogLine;
		private readonly int _maximumDataSourceCharacters;
		private readonly double _maximumWidth;
		private double _yOffset;
		private IDataSource _dataSource;
		private LogFileSection _visibleSection;

		public DataSourceCanvas()
		{
			_dataSourcesPerLogLine = new List<FormattedText>();
			_characterCodesPerDataSource = new List<string>();
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
				if (displayMode == DataSourceDisplayMode.CharacterCode)
					UpdateCharacterCodes(dataSources);

				int actualCharacterCount = 0;

				_dataSourcesPerLogLine.Clear();
				var logLines = new LogLine[visibleSection.Count];
				merged.UnfilteredLogFile.GetSection(visibleSection, logLines);
				foreach (var logLine in logLines)
				{
					var dataSourceId = (int) logLine.SourceId;
					if (dataSourceId >= 0 && dataSourceId < dataSources.Count)
					{
						var text = CreateFormattedText(GetDataSourceName(displayMode, dataSources, dataSourceId));
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

		private void UpdateCharacterCodes(IReadOnlyList<IDataSource> dataSources)
		{
			for (var i = 0; i < dataSources.Count; ++i)
				if (i >= _characterCodesPerDataSource.Count)
					_characterCodesPerDataSource.Add(GenerateCharacterCode(i));
				else
					_characterCodesPerDataSource[i] = GenerateCharacterCode(i);

			var toRemove = _characterCodesPerDataSource.Count - dataSources.Count;
			if (toRemove > 0)
				_characterCodesPerDataSource.RemoveRange(_characterCodesPerDataSource.Count - toRemove, toRemove);
		}

		/// <summary>
		///     Generates the character code for the n-th data source.
		/// </summary>
		/// <param name="dataSourceIndex"></param>
		/// <returns></returns>
		[Pure]
		internal static string GenerateCharacterCode(int dataSourceIndex)
		{
			var builder = new StringBuilder(capacity: 2);
			do
			{
				var value = (char) ('A' + dataSourceIndex % 26);
				builder.Append(value);
				dataSourceIndex /= 26;
			} while (dataSourceIndex > 0);
			return builder.ToString();
		}

		/// <summary>
		///     Returns the name of the data source with the given index, depending on the given display mode.
		/// </summary>
		/// <param name="displayMode"></param>
		/// <param name="dataSources"></param>
		/// <param name="dataSourceIndex"></param>
		/// <returns></returns>
		[Pure]
		private string GetDataSourceName(DataSourceDisplayMode displayMode, IReadOnlyList<IDataSource> dataSources,
			int dataSourceIndex)
		{
			switch (displayMode)
			{
				case DataSourceDisplayMode.CharacterCode:
					return _characterCodesPerDataSource[dataSourceIndex];

				case DataSourceDisplayMode.Filename:
					if (dataSourceIndex >= 0 && dataSourceIndex < dataSources.Count)
					{
						var dataSource = dataSources[dataSourceIndex];
						var fullFileName = dataSource.FullFileName;
						return Path.GetFileName(fullFileName);
					}
					return null;

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