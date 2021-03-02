using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using log4net;
using Tailviewer.Api;

namespace Tailviewer.Ui.SidePanel.Outline
{
	/// <summary>
	///     Responsible for wrapping a <see cref="ILogFileOutlineViewModel" /> and catching all of its unhandled
	///     exceptions, if necessary.
	/// </summary>
	internal sealed class LogFileOutlineViewModelProxy
		: IInternalLogFileOutlineViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly ILogFileOutlineViewModel _innerViewModel;

		private readonly ILogFileOutlinePlugin _plugin;
		private readonly IServiceContainer _services;

		public LogFileOutlineViewModelProxy(ILogFileOutlinePlugin plugin, IServiceContainer services, ILogSource logSource)
		{
			_plugin = plugin;
			_services = services;
			_innerViewModel = TryCreateViewModel(services, plugin, logSource);
		}

		#region Implementation of INotifyPropertyChanged

		#pragma warning disable 67
		public event PropertyChangedEventHandler PropertyChanged;
		#pragma warning restore 67

		#endregion

		private static ILogFileOutlineViewModel TryCreateViewModel(IServiceContainer services,
		                                                           ILogFileOutlinePlugin plugin,
		                                                           ILogSource logSource)
		{
			try
			{
				return plugin.CreateViewModel(services, logSource);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return null;
			}
		}

		public FrameworkElement TryCreateContent()
		{
			if (_innerViewModel == null)
				return null;

			try
			{
				return _plugin.CreateContentPresenterFor(_services, _innerViewModel);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return null;
			}
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			try
			{
				_innerViewModel?.Dispose();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		public void Update()
		{
			try
			{
				_innerViewModel?.Update();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		#endregion
	}
}