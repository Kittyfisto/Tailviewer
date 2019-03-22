using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using log4net;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Templates.Analysis;
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
		private readonly IWidgetTemplate _template;
		private readonly ILogAnalyserConfiguration _analyserConfiguration;
		private bool _isEditing;

		/// <summary>
		/// </summary>
		protected AbstractWidgetViewModel(IWidgetTemplate template,
			IDataSourceAnalyser dataSourceAnalyser)
		{
			if (dataSourceAnalyser == null)
				throw new ArgumentNullException(nameof(dataSourceAnalyser));

			_dataSourceAnalyser = dataSourceAnalyser;
			_analyserConfiguration = dataSourceAnalyser.Configuration?.Clone() as ILogAnalyserConfiguration;
			_template = template;
			CanBeEdited = AnalyserConfiguration != null && !dataSourceAnalyser.IsFrozen;
		}

		/// <summary>
		///     The current configuration of the analysis.
		///     The values of this configuration shall be displayed when <see cref="IsEditing" /> is set to true.
		///     When <see cref="IsEditing" /> is set to false again, the current value of this property is then forwarded
		///     to the <see cref="IDataSourceAnalyser" /> via <see cref="IDataSourceAnalyser.Configuration" />.
		/// </summary>
		protected ILogAnalyserConfiguration AnalyserConfiguration => _analyserConfiguration;

		/// <summary>
		///     The current configuration of the view.
		/// </summary>
		protected IWidgetConfiguration ViewConfiguration => _template.Configuration;

		/// <inheritdoc />
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

		/// <inheritdoc />
		public bool CanBeEdited { get; }

		/// <inheritdoc />
		public string Title
		{
			get { return _template.Title; }
			set
			{
				if (value == _template.Title)
					return;

				_template.Title = value;
				EmitPropertyChanged();
			}
		}

		/// <inheritdoc />
		public IWidgetTemplate Template => _template;

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <inheritdoc />
		public virtual void Update()
		{
			
		}

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

		/// <summary>
		///     Call this method when a publicly visible property has changed its value.
		/// </summary>
		/// <param name="propertyName"></param>
		protected virtual void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		///     Tries to get the latest result.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="result"></param>
		/// <returns></returns>
		protected bool TryGetResult<T>(out T result) where T : class, ILogAnalysisResult
		{
			result = _dataSourceAnalyser.Result as T;
			return result != null;
		}
	}
}