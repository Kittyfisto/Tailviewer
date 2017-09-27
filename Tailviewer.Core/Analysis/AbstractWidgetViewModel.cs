using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Core.Analysis
{
	/// <summary>
	///     The base class for all <see cref="IWidgetViewModel" /> implementations.
	/// </summary>
	public abstract class AbstractWidgetViewModel
		: IWidgetViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IDataSourceAnalyser _dataSourceAnalyser;
		private bool _isAnalysisFinished;

		private bool _isEditing;
		private double _progress;
		private string _progressTooltip;
		private string _title;

		/// <summary>
		/// </summary>
		protected AbstractWidgetViewModel(IDataSourceAnalyser dataSourceAnalyser, IWidgetConfiguration viewConfiguration)
		{
			if (dataSourceAnalyser == null)
				throw new ArgumentNullException(nameof(dataSourceAnalyser));

			_dataSourceAnalyser = dataSourceAnalyser;
			_isAnalysisFinished = false;
			AnalyserConfiguration = CloneConfiguration(dataSourceAnalyser);
			ViewConfiguration = viewConfiguration;
			CanBeEdited = AnalyserConfiguration != null && !dataSourceAnalyser.IsFrozen;
			DeleteCommand = new DelegateCommand(Delete);
		}

		/// <summary>
		///     The current configuration of the analysis.
		///     The values of this configuration shall be displayed when <see cref="IsEditing" /> is set to true.
		///     When <see cref="IsEditing" /> is set to false again, the current value of this property is then forwarded
		///     to the <see cref="IDataSourceAnalyser" /> via <see cref="IDataSourceAnalyser.Configuration" />.
		/// </summary>
		protected ILogAnalyserConfiguration AnalyserConfiguration { get; }

		/// <summary>
		///     The current configuration of the view.
		/// </summary>
		protected IWidgetConfiguration ViewConfiguration { get; }

		public bool IsEditing
		{
			get { return _isEditing; }
			set
			{
				if (value == _isEditing)
					return;

				_isEditing = value;
				EmitPropertyChanged();

				if (!value)
					_dataSourceAnalyser.Configuration = AnalyserConfiguration.Clone() as ILogAnalyserConfiguration;
			}
		}

		public bool CanBeEdited { get; }

		public string Title
		{
			get { return _title; }
			set
			{
				if (value == _title)
					return;

				_title = value;
				EmitPropertyChanged();
			}
		}

		public ICommand DeleteCommand { get; }

		public bool IsAnalysisFinished
		{
			get { return _isAnalysisFinished; }
			private set
			{
				if (value == _isAnalysisFinished)
					return;

				_isAnalysisFinished = value;
				EmitPropertyChanged();
			}
		}

		public double Progress
		{
			get { return _progress; }
			protected set
			{
				if (double.IsNaN(value))
					value = 1;

				if (value == _progress)
					return;

				_progress = value;
				EmitPropertyChanged();

				IsAnalysisFinished = value >= 1;
				ProgressTooltip = string.Format("Analysis {0:P} complete", value);
			}
		}

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

		public event PropertyChangedEventHandler PropertyChanged;

		public virtual void Update()
		{
			Progress = _dataSourceAnalyser.Progress.RelativeValue;
		}

		public event Action<IWidgetViewModel> OnDelete;

		private ILogAnalyserConfiguration CloneConfiguration(IDataSourceAnalyser dataSourceAnalyser)
		{
			try
			{
				return (ILogAnalyserConfiguration) dataSourceAnalyser.Configuration?.Clone();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while cloning configuration: {0}", e);
				return null;
			}
		}

		protected bool TryGetResult<T>(out T result) where T : class, ILogAnalysisResult
		{
			result = _dataSourceAnalyser.Result as T;
			return result != null;
		}

		private void Delete()
		{
			OnDelete?.Invoke(this);
		}

		protected virtual void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}