using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.SystemTests.Plugins.v0._8._0._680
{
	[TestFixture]
	public sealed class PluginCompatibilityTest
	{
		[Test]
		public void TestLoadFileFormatPlugin()
		{
			Load<IFileFormatPlugin>(new Version(0, 8, 0, 680), "FileFormatPlugin.0.0.tvp");
		}

		private static void Load<T>(Version tailviewerVersion, string pluginName) where T : IPlugin
		{
			var pluginPath = Path.Combine(PluginsDirectory, "v"+tailviewerVersion.ToString(), pluginName);

			var argumentBuilder = new StringBuilder();
			argumentBuilder.AppendFormat("/testloadplugin \"{0}\" {1}", pluginPath, typeof(T).FullName);

			using (var process = new Process())
			{
				process.StartInfo = new ProcessStartInfo("Tailviewer.exe")
				{
					Arguments = argumentBuilder.ToString(),
					RedirectStandardOutput = true,
					CreateNoWindow = true,
					UseShellExecute = false
				};

				process.Start().Should().BeTrue();

				var output = process.StandardOutput.ReadToEnd();
				process.WaitForExit();

				var exitCode = process.ExitCode;
				if (exitCode != 0)
				{
					TestContext.Progress.WriteLine(output);
					exitCode.Should().Be(0);
				}

				output.Should().NotContain("Error");
			}
		}

		private static string PluginsDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				var assemblyPath =  Path.GetDirectoryName(path);
				var projectPath = Path.Combine(assemblyPath, "..", "src", "Tailviewer.SystemTests", "Plugins");
				return projectPath;
			}
		}
	}
}
