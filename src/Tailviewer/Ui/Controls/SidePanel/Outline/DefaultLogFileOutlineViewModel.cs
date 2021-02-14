using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using log4net;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Ui.Properties;

namespace Tailviewer.Ui.Controls.SidePanel.Outline
{
	internal sealed class DefaultLogFileOutlineViewModel
		: IInternalLogFileOutlineViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogFile _logFile;
		private readonly LogFilePropertyList _propertyValues;
		private readonly Dictionary<IReadOnlyPropertyDescriptor, IPropertyPresenter> _presentersByProperty;
		private readonly ObservableCollection<IPropertyPresenter> _presenters;
		private readonly IPropertyPresenterPlugin _registry;

		public DefaultLogFileOutlineViewModel(IServiceContainer serviceContainer, ILogFile logFile)
		{
			_logFile = logFile;
			_propertyValues = new LogFilePropertyList();
			_presentersByProperty = new Dictionary<IReadOnlyPropertyDescriptor, IPropertyPresenter>();
			_presenters = new ObservableCollection<IPropertyPresenter>();
			_registry = serviceContainer.TryRetrieve<IPropertyPresenterPlugin>();
			if (_registry == null)
			{
				Log.WarnFormat("Registry is missing: No properties will be displayed...");
			}
		}

		public IReadOnlyList<IPropertyPresenter> Properties
		{
			get { return _presenters; }
		}

		public void Update()
		{
			_logFile.GetAllProperties(_propertyValues);
			foreach (var property in _propertyValues.Properties)
			{
				if (!_presentersByProperty.TryGetValue(property, out var presenter
				                                      ))
				{
					presenter = TryCreateViewModel(property);
					_presentersByProperty.Add(property, presenter
					                         );

					if (presenter != null)
						_presenters.Add(presenter);
				}

				// Some properties just don't have presenters and we want to avoid trying to create one over and over,
				// so we simply remember those that don't have one
				presenter?.Update(_propertyValues.GetValue(property));
			}
		}

		#region Implementation of INotifyPropertyChanged

#pragma warning disable 67
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67

		#endregion

		#region Implementation of IDisposable

		public void Dispose()
		{
		}

		public FrameworkElement TryCreateContent()
		{
			return new DefaultLogFileOutline
			{
				DataContext = this
			};
		}

		#endregion

		private IPropertyPresenter TryCreateViewModel(IReadOnlyPropertyDescriptor property)
		{
			try
			{
				return _registry?.TryCreatePresenterFor(property);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return null;
			}
		}
	}
}