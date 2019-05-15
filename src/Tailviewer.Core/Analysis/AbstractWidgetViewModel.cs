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
		private bool _isEditing;
		private bool _canBeEdited;

		/// <summary>
		/// </summary>
		protected AbstractWidgetViewModel(IWidgetTemplate template,
			IDataSourceAnalyser dataSourceAnalyser)
		{
			if (dataSourceAnalyser == null)
				throw new ArgumentNullException(nameof(dataSourceAnalyser));

			_dataSourceAnalyser = dataSourceAnalyser;
			AnalyserConfiguration = CloneConfiguration(dataSourceAnalyser);
			Template = template;
			CanBeEdited = AnalyserConfiguration != null && !dataSourceAnalyser.IsFrozen;
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
		protected IWidgetConfiguration ViewConfiguration => Template.Configuration;

		/// <inheritdoc />
		public bool IsEditing
		{
			get => _isEditing;
			set
			{
				if (value == _isEditing)
					return;

				_isEditing = value;
				EmitPropertyChanged();

				if (!value)
				{
					var existingConfiguration = _dataSourceAnalyser.Configuration;
					var newConfiguration = AnalyserConfiguration;
					if (existingConfiguration != null || newConfiguration != null)
					{
						_dataSourceAnalyser.Configuration = newConfiguration?.Clone() as ILogAnalyserConfiguration;
						EmitTemplateModified();
					}
				}
			}
		}

		/// <inheritdoc />
		public bool CanBeEdited
		{
			get => _canBeEdited;
			protected set
			{
				if (value == _canBeEdited)
					return;

				_canBeEdited = value;
				EmitPropertyChanged();
			}
		}

		/// <inheritdoc />
		public string Title
		{
			get => Template.Title;
			set
			{
				if (value == Template.Title)
					return;

				Template.Title = value;
				EmitPropertyChanged();

				EmitTemplateModified();
			}
		}

		/// <inheritdoc />
		public IWidgetTemplate Template { get; }

		/// <inheritdoc />
		public event Action TemplateModified;

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <inheritdoc />
		public virtual void Update()
		{
		}

		/// <summary>
		///     Fires the <see cref="TemplateModified" /> event.
		/// </summary>
		protected void EmitTemplateModified()
		{
			TemplateModified?.Invoke();
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