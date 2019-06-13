using System.Diagnostics;
using System.IO;
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
		[Ignore("not yet implemented")]
		public void TestLoadFileFormatPlugin()
		{
			Load<IFileFormatPlugin>(@"C:\Users\Simon\Documents\GitHub\Tailviewer\src\Tailviewer.SystemTests\Plugins\v0.8.0.680\FileFormatPlugin.0.8.0.680.tvp");
		}

		private static void Load<T>(string pluginPath) where T : IPlugin
		{
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
			}
		}
	}
}
