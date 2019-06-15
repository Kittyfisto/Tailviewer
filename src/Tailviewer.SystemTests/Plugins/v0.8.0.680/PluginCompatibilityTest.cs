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
			// CMD> Tailviewer.PluginCreator.exe FileFormatPlugin  Tailviewer.BusinessLogic.Plugins.IFileFormatPlugin 0.8.0.680-beta
			Load<IFileFormatPlugin>(new Version(0, 8, 0, 680), "FileFormatPlugin.0.0.tvp");
		}

		[Test]
		public void TestLoadFileFormatPluginAndLogFileImplementation()
		{
			// CMD> Tailviewer.PluginCreator.exe FileFormatPlugin+LogFile  Tailviewer.BusinessLogic.Plugins.IFileFormatPlugin,Tailviewer.BusinessLogic.LogFiles.ILogFile 0.8.0.680-beta
			Load<IFileFormatPlugin>(new Version(0, 8, 0, 680), "FileFormatPlugin+LogFile.0.0.tvp");
		}

		private static void Load<T>(Version tailviewerVersion, string pluginName) where T : IPlugin
		{
			var pluginPath = Path.Combine(PluginsDirectory, "v"+tailviewerVersion, pluginName);

			const string executable = "Tailviewer.exe";
			var argumentBuilder = new StringBuilder();
			argumentBuilder.AppendFormat("/testloadplugin \"{0}\" {1}", pluginPath, typeof(T).FullName);

			TestContext.Progress.WriteLine("{0} {1}", executable, argumentBuilder);

			using (var process = new Process())
			{
				process.StartInfo = new ProcessStartInfo(executable)
				{
					Arguments = argumentBuilder.ToString(),
					RedirectStandardOutput = true,
					CreateNoWindow = true,
					UseShellExecute = false
				};

				process.Start().Should().BeTrue();

				var output = process.StandardOutput.ReadToEnd();
				process.WaitForExit();
				TestContext.Progress.WriteLine(output);

				var exitCode = process.ExitCode;
				if (exitCode != 0)
				{
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
