using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.Analysis.Analysers;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets
{
	/// <summary>
	///     The base class for all <see cref="IWidgetViewModel" /> implementations.
	/// </summary>
	public abstract class AbstractWidgetViewModel
		: IWidgetViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IDataSourceAnalyser _dataSourceAnalyser;

		private bool _isEditing;
		private string _title;

		/// <summary>
		/// </summary>
		protected AbstractWidgetViewModel(IDataSourceAnalyser dataSourceAnalyser)
		{
			if (dataSourceAnalyser == null)
				throw new ArgumentNullException(nameof(dataSourceAnalyser));

			_dataSourceAnalyser = dataSourceAnalyser;
			CanBeEdited = dataSourceAnalyser.Configuration != null && !dataSourceAnalyser.IsFrozen;
			DeleteCommand = new DelegateCommand(Delete);
		}

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
				{
					// For now, we'll only update the configuration when the edit
					// mode is disabled.
					UpdateConfiguration();
				}
			}
		}

		private void UpdateConfiguration()
		{
			try
			{
				var configuration = Configuration;
				_dataSourceAnalyser.Configuration = configuration;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		protected abstract ILogAnalyserConfiguration Configuration { get; }

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

		public event PropertyChangedEventHandler PropertyChanged;

		public abstract void OnUpdate();

		public event Action<IWidgetViewModel> OnDelete;

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