using System;
using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.Plugins
{
	public interface IPluginDescription
	{
		string FilePath { get; }

		/// <summary>
		/// 
		/// </summary>
		string Author { get; }

		/// <summary>
		/// 
		/// </summary>
		string Description { get; }

		/// <summary>
		/// 
		/// </summary>
		Uri Website { get; }

		/// <summary>
		///     A map from the plugin interface to its actual implementation.
		/// </summary>
		IReadOnlyDictionary<Type, string> Plugins { get; }
	}
}