using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Media;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	///     Describes a plugin that is located inside a .NET assembly.
	/// </summary>
	public sealed class PluginDescription
		: IPluginDescription
	{
		private static readonly ReadOnlyDictionary<Type, string> NoPlugins;

		static PluginDescription()
		{
			NoPlugins = new ReadOnlyDictionary<Type, string>(new Dictionary<Type, string>());
		}

		/// <summary>
		///     Initializes this plugin descriptions.
		/// </summary>
		public PluginDescription()
		{
			Plugins = NoPlugins;
		}

		/// <inheritdoc />
		public string FilePath { get; set; }

		/// <inheritdoc />
		public string Id { get; set; }

		/// <inheritdoc />
		public string Name { get; set; }

		/// <inheritdoc />
		public string Author { get; set; }

		/// <inheritdoc />
		public string Description { get; set; }

		/// <inheritdoc />
		public Version Version { get; set; }

		/// <inheritdoc />
		public Uri Website { get; set; }

		/// <inheritdoc />
		public ImageSource Icon { get; set; }

		/// <inheritdoc />
		public string Error { get; set; }

		/// <inheritdoc />
		public IReadOnlyDictionary<Type, string> Plugins { get; set; }

		#region Overrides of Object

		public override string ToString()
		{
			var builder = new StringBuilder();

			if (Name != null)
			{
				if (builder.Length != 0)
					builder.Append(", ");
				builder.AppendFormat("Name: {0}", Name);
			}

			if (Version != null)
			{
				if (builder.Length != 0)
					builder.Append(", ");
				builder.AppendFormat("Version: {0}", Version);
			}

			if (Author != null)
			{
				if (builder.Length != 0)
					builder.Append(", ");
				builder.AppendFormat("Author: {0}", Author);
			}

			if (builder.Length == 0)
			{
				builder.Append("<Unknown>");
			}

			return builder.ToString();
		}

		#endregion
	}
}