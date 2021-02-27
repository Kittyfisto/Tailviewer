using System;
using System.Diagnostics;
using System.Text;

namespace Tailviewer.PluginCreator
{
    internal sealed class MsBuild
    {
        public void Build(string projectFile, string configuration)
        {
            const string msbuildPath =
                @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe";

            var argumentBuilder = new StringBuilder();
            argumentBuilder.AppendFormat("\"{0}\" /t:Build /p:Configuration={1}", projectFile, configuration);

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(msbuildPath)
                {
                    Arguments = argumentBuilder.ToString(),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                if (!process.Start())
                    throw new NotImplementedException();

                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                var exitCode = process.ExitCode;
                if (exitCode != 0)
                {
                    Console.WriteLine(output);
                    throw new Exception($"msbuild returned {exitCode}");
                }
            }
        }
    }
}