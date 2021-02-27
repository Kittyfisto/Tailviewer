using System.Windows;
using Tailviewer.Ui;

namespace Tailviewer.Plugins
{
	/// <summary>
	///     The plugin interface for a non-file based data source which nonetheless contains log events.
	/// </summary>
	/// <remarks>
	///     File-less data sources are usually only accessible via some sort of API, be it an operating system
	///     API or something more specific. Either way, a plugin must be implemented which takes care of accessing
	///     this API and converting its log events into a format which tailviewer can understand, namely <see cref="ILogEntry"/>.
	/// </remarks>
	/// <example>
	///     An example implementation can be found in the 'Tailviewer.DataSources.UDP' plugin which is part of the
	///     Tailviewer repository. This plugin takes care of interpreting a stream of UDP datagrams send to a particular
	///     end-point as log events where each datagram contains a singular log event in plain text.
	/// </example>
	[PluginInterfaceVersion(1)]
	public interface ICustomDataSourcePlugin
		: IPlugin
	{
		/// <summary>
		///     The name of this custom data source.
		/// </summary>
		/// <remarks>
		///     This name is presented to users when they select which custom data source to add to tailviewer and should
		///     be understandable enough for them to make a meaningful decision.
		/// </remarks>
		string DisplayName { get; }

		/// <summary>
		///    Uniquely identifies this implementation.
		/// </summary>
		CustomDataSourceId Id { get; }

		/// <summary>
		/// </summary>
		/// <remarks>
		///     Tailviewer will call this method whenever the a new data source is created or an existing data source
		///     is restored, for example after tailviewer is started. Tailviewer may invoke
		///     <see cref="ICustomDataSourceConfiguration.Restore" />
		///     right after construction in case the data source already existed (or its configuration was stored on disk).
		///     Either way, the object created through this method will then be forwarded to <see cref="CreateViewModel" />
		///     and <see cref="CreateLogSource" /> at the appropriate time.
		/// </remarks>
		/// <param name="serviceContainer">The service container containing a myriad of services which this plugin may access</param>
		/// <returns></returns>
		ICustomDataSourceConfiguration CreateConfiguration(IServiceContainer serviceContainer);

		/// <summary>
		///     Creates a new view model which allows the user to view and edit the configuration of the data source.
		/// </summary>
		/// <example>
		/// </example>
		/// <param name="serviceContainer">The service container containing a myriad of services which this plugin may access</param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		ICustomDataSourceViewModel CreateViewModel(IServiceContainer serviceContainer,
		                                           ICustomDataSourceConfiguration configuration);

		/// <summary>
		///     Creates a new control which realizes the input elements to allow a user to view and edit the configuration.
		/// </summary>
		/// <remarks>
		///     Tailviewer will call this method with a view model previously created with
		///     <see cref="CreateViewModel" />. This implementation needs to take care of binding the
		///     properties of the view model to the created control!
		/// </remarks>
		/// <param name="serviceContainer">The service container containing a myriad of services which this plugin may access</param>
		/// <param name="viewModel"></param>
		/// <returns></returns>
		FrameworkElement CreateConfigurationControl(IServiceContainer serviceContainer,
		                                            ICustomDataSourceViewModel viewModel);

		/// <summary>
		///     Creates a new <see cref="ILogSource" /> instance which provides the data source's log events to tailviewer.
		/// </summary>
		/// <remarks>
		///     The implementation will have to make sure to convert the data source's log events to tailviewer's interface
		///     and to notify tailviewer whenever the data source changes.
		/// </remarks>
		/// <param name="serviceContainer">The service container containing a myriad of services which this plugin may access</param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		ILogSource CreateLogSource(IServiceContainer serviceContainer,
		                           ICustomDataSourceConfiguration configuration);
	}
} 