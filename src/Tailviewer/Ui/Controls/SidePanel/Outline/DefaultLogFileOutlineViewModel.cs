using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using log4net;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Ui.Controls.SidePanel.Outline
{
	internal sealed class DefaultLogFileOutlineViewModel
		: IInternalLogFileOutlineViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogFile _logFile;
		private readonly IReadOnlyList<ILogFilePropertyViewModel> _properties;

		public DefaultLogFileOutlineViewModel(ILogFile logFile)
		{
			_logFile = logFile;
			var properties = logFile.Properties ?? Enumerable.Empty<ILogFilePropertyDescriptor>();
			_properties = properties.Select(TryCreateViewModel)
			                     .Where(x => x != null)
			                     .ToList();
		}

		public IReadOnlyList<ILogFilePropertyViewModel> Properties
		{
			get { return _properties; }
		}

		#region Implementation of INotifyPropertyChanged

#pragma warning disable 67
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67

		#endregion

		private ILogFilePropertyViewModel TryCreateViewModel(ILogFilePropertyDescriptor x)
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

		private LogFilePropertyViewModel<T> CreateViewModel<T>(ILogFilePropertyDescriptor<T> descriptor)
		{
			return new LogFilePropertyViewModel<T>(descriptor);
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
		}

		public void Update()
		{
			foreach (var property in _properties)
				property.Update(_logFile);
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