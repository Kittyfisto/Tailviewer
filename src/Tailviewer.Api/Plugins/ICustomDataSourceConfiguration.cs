using System;
using System.Xml;

namespace Tailviewer.Plugins
{
	/// <summary>
	///     The interface to hold the configuration necessary to access a custom data source.
	/// </summary>
	/// <remarks>
	///     When tailviewer stores this configuration, it first clones it on the UI thread
	///     (via <see cref="ICloneable.Clone"/>) and then eventually calls <see cref="Store"/>
	///     on the cloned object on a background thread prior to writing the configuration to disk.
	///
	///     Restoring is generally only done once while the application is started and happens on an unspecified thread.
	/// </remarks>
	public interface ICustomDataSourceConfiguration
		: ICloneable
	{
		/// <summary>
		///     Is called usually after construction in case a previous data source is restored, for example
		///     if tailviewer was just started and the custom data source was previously stored.
		///     Otherwise, this method is not called.
		/// </summary>
		/// <remarks>
		///     An implementation should try to restore its configuration from the given blob or throw an exception
		///     in case that is not possible.
		/// </remarks>
		/// <param name="reader"></param>
		void Restore(XmlReader reader);

		/// <summary>
		///     Is called by tailviewer whenever it deems it necessary to store the custom data source's configuration.
		/// </summary>
		/// <remarks>
		///     An implementation should try to store the important information that shall be persisted in between tailviewer
		///     sessions in the given writer. Information which is not written to the writer (and then not later restored via
		///     <see cref="Restore" /> will be lost!).
		/// </remarks>
		/// <param name="writer"></param>
		/// s
		/// <returns></returns>
		void Store(XmlWriter writer);
	}
}