using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Templates.Analysis;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets
{
	/// <summary>
	///     Responsible for encapsulating a shitty <see cref="IWidgetViewModel" /> implementation, handling every exception
	///     it might throw.
	/// </summary>
	public sealed class WidgetViewModelProxy
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IWidgetPlugin _plugin;
		private readonly IWidgetTemplate _widgetTemplate;
		private readonly IDataSourceAnalyser _analyser;
		private readonly DelegateCommand _deleteCommand;

		private bool _isAnalysisFinished;
		private double _progress;
		private string _progressTooltip;

		public WidgetViewModelProxy(IWidgetPlugin plugin, IWidgetTemplate widgetTemplate, IDataSourceAnalyser analyser)
		{
			_plugin = plugin;
			_widgetTemplate = widgetTemplate;
			_analyser = analyser;
			_deleteCommand = new DelegateCommand(Delete);

			try
			{
				InnerViewModel = plugin.CreateViewModel(widgetTemplate, analyser);
			}
			catch (Exception e)
			{
				var type = plugin.GetType();
				Log.ErrorFormat("{0}.CreateViewModel() threw exception:\r\n{1}", type.FullName, e);
			}

			if (InnerViewModel != null)
			{
				InnerViewModel.PropertyChanged += ViewModelOnPropertyChanged;

				try
				{
					Content = plugin.CreateContentPresenterFor(InnerViewModel);
				}
				catch (Exception e)
				{
					var type = plugin.GetType();
					Log.ErrorFormat("{0}.CreateContentPresenterFor() threw exception:\r\n{1}", type.FullName, e);
				}
			}
		}

		private void Delete()
		{
			OnDelete?.Invoke(this);
		}

		public IWidgetViewModel InnerViewModel { get; }

		public event PropertyChangedEventHandler PropertyChanged;

		public bool IsEditing
		{
			get
			{
				try
				{
					return InnerViewModel?.IsEditing ?? false;
				}
				catch (Exception e)
				{
					var type = InnerViewModel.GetType();
					Log.ErrorFormat("{0}.get_IsEditing() threw exception:\r\n{1}", type.FullName, e);

					return false;
				}
			}
			set
			{
				try
				{
					if (InnerViewModel != null)
						InnerViewModel.IsEditing = value;
				}
				catch (Exception e)
				{
					var type = InnerViewModel.GetType();
					Log.ErrorFormat("{0}.set_IsEditing() threw exception:\r\n{1}", type.FullName, e);
				}
			}
		}

		public bool CanBeEdited
		{
			get
			{
				try
				{
					return InnerViewModel?.CanBeEdited ?? false;
				}
				catch (Exception e)
				{
					var type = InnerViewModel.GetType();
					Log.ErrorFormat("{0}.get_CanBeEdited() threw exception:\r\n{1}", type.FullName, e);

					return false;
				}
			}
		}

		public string Title
		{
			get
			{
				try
				{
					return InnerViewModel?.Title;
				}
				catch (Exception e)
				{
					var type = InnerViewModel.GetType();
					Log.ErrorFormat("{0}.get_Title() threw exception:\r\n{1}", type.FullName, e);

					return "<Unnamed>";
				}
			}
			set
			{
				try
				{
					if(InnerViewModel != null)
						InnerViewModel.Title = value;
				}
				catch (Exception e)
				{
					var type = InnerViewModel.GetType();
					Log.ErrorFormat("{0}.set_Title() threw exception:\r\n{1}", type.FullName, e);
				}
			}
		}

		/// <summary>
		/// The control which presents the <see cref="InnerViewModel"/>.
		/// </summary>
		public FrameworkElement Content { get; }

		/// <summary>
		///     The command to delete this widget.
		/// </summary>
		public ICommand DeleteCommand => _deleteCommand;

		/// <summary>
		///     Whether or not the analysis has been finished.
		///     The analysis is only ever finished when <see cref="Progress" /> has reached
		///     <see cref="Percentage.HundredPercent" />.
		/// </summary>
		public bool IsAnalysisFinished
		{
			get => _isAnalysisFinished;
			private set
			{
				if (value == _isAnalysisFinished)
					return;

				_isAnalysisFinished = value;
				EmitPropertyChanged();
			}
		}

		/// <summary>
		///     The current progress of the analysis.
		/// </summary>
		public double Progress
		{
			get => _progress;
			private set
			{
				if (double.IsNaN(value))
					value = 1;

				if (value == _progress)
					return;

				_progress = value;
				EmitPropertyChanged();

				IsAnalysisFinished = value >= 1;
				ProgressTooltip = $"Analysis {value:P} complete";
			}
		}

		/// <summary>
		/// Tooltip describing the current progress.
		/// </summary>
		public string ProgressTooltip
		{
			get { return _progressTooltip; }
			private set
			{
				if (value == _progressTooltip)
					return;

				_progressTooltip = value;
				EmitPropertyChanged();
			}
		}

		public IWidgetTemplate Template
		{
			get
			{
				try
				{
					return InnerViewModel?.Template;
				}
				catch (Exception e)
				{
					var type = InnerViewModel.GetType();
					Log.ErrorFormat("{0}.get_Template() threw exception:\r\n{1}", type.FullName, e);

					return null;
				}
			}
		}

		public void Update()
		{
			UpdateProgress();
			UpdateViewModel();
		}

		/// <summary>
		///     This event is fired when the <see cref="DeleteCommand" /> is executed.
		/// </summary>
		public event Action<WidgetViewModelProxy> OnDelete;

		private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			PropertyChanged?.Invoke(this, e);
		}

		/// <summary>
		///     Call this method when a publicly visible property has changed its value.
		/// </summary>
		/// <param name="propertyName"></param>
		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void UpdateProgress()
		{
			try
			{
				Progress = _analyser.Progress.RelativeValue;
			}
			catch (Exception e)
			{
				var type = _analyser.GetType();
				Log.ErrorFormat("{0}.get_Progress() threw exception:\r\n{1}", type.FullName, e);
			}
		}

		private void UpdateViewModel()
		{
			try
			{
				InnerViewModel?.Update();
			}
			catch (Exception e)
			{
				var type = InnerViewModel.GetType();
				Log.ErrorFormat("{0}.Update() threw exception:\r\n{1}", type.FullName, e);
			}
		}
	}
}