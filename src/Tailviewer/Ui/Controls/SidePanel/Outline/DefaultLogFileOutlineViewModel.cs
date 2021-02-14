using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using log4net;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Ui.Controls.SidePanel.Outline
{
	internal sealed class DefaultLogFileOutlineViewModel
		: IInternalLogFileOutlineViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogFile _logFile;
		private readonly ILogFileProperties _propertyValues;
		private readonly Dictionary<IReadOnlyPropertyDescriptor, ILogFilePropertyViewModel> _viewModelsByProperty;
		private readonly ObservableCollection<ILogFilePropertyViewModel> _viewModels;

		public DefaultLogFileOutlineViewModel(ILogFile logFile)
		{
			_logFile = logFile;
			_propertyValues = new LogFilePropertyList();
			_viewModelsByProperty = new Dictionary<IReadOnlyPropertyDescriptor, ILogFilePropertyViewModel>();
			_viewModels = new ObservableCollection<ILogFilePropertyViewModel>();
		}

		public IReadOnlyList<ILogFilePropertyViewModel> Properties
		{
			get { return _viewModels; }
		}

		#region Implementation of INotifyPropertyChanged

#pragma warning disable 67
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67

		#endregion

		private ILogFilePropertyViewModel TryCreateViewModel(IReadOnlyPropertyDescriptor x)
		{
			try
			{
				return (ILogFilePropertyViewModel) CreateViewModel((dynamic) x);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return null;
			}
		}

		private LogFilePropertyViewModel<T> CreateViewModel<T>(IReadOnlyPropertyDescriptor<T> descriptor)
		{
			return new LogFilePropertyViewModel<T>(descriptor);
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
		}

		public void Update()
		{
			_logFile.GetAllProperties(_propertyValues);
			foreach (var property in _propertyValues.Properties)
			{
				if (!_viewModelsByProperty.TryGetValue(property, out var viewModel))
				{
					viewModel = TryCreateViewModel(property);
					_viewModelsByProperty.Add(property, viewModel);
					_viewModels.Add(viewModel);
				}
				viewModel.Update(_propertyValues);
			}
		}

		public FrameworkElement TryCreateContent()
		{
			return new DefaultLogFileOutline
			{
				DataContext = this
			};
		}

		#endregion
	}
}