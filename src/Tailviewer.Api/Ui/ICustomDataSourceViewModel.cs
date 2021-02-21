using System;

namespace Tailviewer.Ui
{
	/// <summary>
	///     This interface should be implemented by plugins to allow a user to view / edit the configuration
	///     of a custom data source.
	/// </summary>
	/// <example>
	///     An example of this implementation can be found in the `Tailviewer.DataSources.UDP` plugin which is part
	///     of this repository.
	/// </example>
	public interface ICustomDataSourceViewModel
	{
		/// <summary>
		///     Whenever this event is fired, tailviewer will store the information contained in this configuration.
		/// </summary>
		/// <remarks>
		///     By default, tailviewer will store the configurations when the application exits normally. This event may
		///     be fired in case this isn't good enough and the information should be stored earlier.
		/// </remarks>
		event Action<ICustomDataSourceViewModel> RequestStore;
	}
}