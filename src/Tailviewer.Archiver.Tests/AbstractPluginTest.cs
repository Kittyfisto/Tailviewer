using System.IO;
using FluentAssertions;

namespace Tailviewer.Archiver.Tests
{
	public abstract class AbstractPluginTest
	{
		protected static void CreatePlugin(string assemblyFileName,
			string author = null,
			string website = null,
			string description = null)
		{
			var pluginName = Path.GetFileNameWithoutExtension(assemblyFileName);
			var idParts = pluginName.Split('.');
			idParts.Should().HaveCount(2);
			var @namespace = idParts[0];
			var name = idParts[1];

			var builder = new PluginBuilder(@namespace, name, pluginName, author, website, description);
			builder.Save();
		}
	}
}