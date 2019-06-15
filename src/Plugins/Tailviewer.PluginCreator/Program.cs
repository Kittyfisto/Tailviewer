using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.PluginCreator
{
	class Program
	{
		static int Main(string[] args)
		{
			try
			{
				Run(args);
				return 0;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return -1;
			}
		}

		private static void Run(string[] args)
		{
			Console.WriteLine("Responsible for writing, compiling and packing Tailviewer plugins.");
			Console.WriteLine("Used in order to quickly create dummy plugins for the system test");

			var pluginName = args[0];
			var interfaces = ResolveInterfaceType(args[1]);
			var tailviewerVersion = args[2];
			var folder = Directory.GetCurrentDirectory();

			Console.WriteLine("Downloading dependencies (v{0})...", tailviewerVersion);
			var dependencies = DownloadDependencies(folder, tailviewerVersion);

			Console.WriteLine("Writing source files....");
			var sourceFiles = GenerateSourceCode(folder, pluginName, interfaces, tailviewerVersion);

			Console.WriteLine("Writing package config");
			var packageConfig = GeneratePackagesConfig(folder);

			Console.WriteLine("Generating project file...");
			var projectFile = GenerateProjectFile(pluginName, folder, packageConfig, sourceFiles, dependencies,
			                                      out var assemblyPath);

			Console.WriteLine("Building plugin...");
			BuildProject(projectFile);

			Console.WriteLine("Packing plugin...");
			var archivePath = PackPluginAssembly(folder, assemblyPath, pluginName);

			Console.WriteLine("The plugin has been written to:");
			Console.WriteLine(archivePath);
		}

		private static IReadOnlyList<string> GenerateSourceCode(string folder,
		                                                        string pluginName,
		                                                        IEnumerable<Type> implementedTypes,
		                                                        string tailviewerVersion)
		{
			var files = new List<string>();

			files.Add(CreateAssemblyInfo(folder, pluginName, tailviewerVersion));

			foreach (var type in implementedTypes)
			{
				if (type == typeof(IFileFormatPlugin))
				{
					files.AddRange(new FileFormatPluginCreator().CreateSourceFiles(folder));
				}
				else if (type == typeof(ILogFile))
				{
					files.AddRange(new LogFileCreator().CreateSourceFiles(folder));
				}
				else if (type == typeof(IDataSourceAnalyserPlugin))
				{
					files.AddRange(new DataSourceAnalyserPluginCreator().CreateSourceFiles(folder));
				}
				else if (type == typeof(IDataSourceAnalyser))
				{
					files.AddRange(new DataSourceAnalyserCreator().CreateSourceFiles(folder));
				}
				else if (type == typeof(ILogAnalyserPlugin))
				{
					files.AddRange(new LogAnalyserPluginCreator().CreateSourceFiles(folder));
				}
				else if (type == typeof(ILogAnalyser))
				{
					files.AddRange(new LogAnalyserCreator().CreateSourceFiles(folder));
				}
				else
				{
					throw new ArgumentNullException();
				}
			}

			return files;
		}

		private static string GeneratePackagesConfig(string folder)
		{
			var builder = new StringBuilder();

			builder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			builder.AppendLine("<packages>");
			builder.AppendLine();
			builder.AppendLine("</packages>");

			var fileName = Path.Combine(folder, "packages.config");
			File.WriteAllText(fileName, builder.ToString());
			return fileName;
		}

		private static string CreateAssemblyInfo(string folder, string pluginName, string tailviewerVersion)
		{
			var builder = new StringBuilder();
			builder.AppendLine("using System.Reflection;");
			builder.AppendLine("using System.Runtime.InteropServices;");
			builder.AppendLine("using Tailviewer.BusinessLogic.Plugins;");
			builder.AppendLine("[assembly: AssemblyTitle(\"Tailviewer.PluginCreator\")]");
			builder.AppendLine("[assembly: AssemblyDescription(\"\")]");
			builder.AppendLine("[assembly: AssemblyConfiguration(\"\")]");
			builder.AppendLine("[assembly: AssemblyCompany(\"\")]");
			builder.AppendLine("[assembly: AssemblyProduct(\"Tailviewer.PluginCreator\")]");
			builder.AppendLine("[assembly: AssemblyCopyright(\"Copyright ©  2019\")]");
			builder.AppendLine("[assembly: AssemblyTrademark(\"\")]");
			builder.AppendLine("[assembly: AssemblyCulture(\"\")]");
			builder.AppendLine("[assembly: ComVisible(false)]");
			builder.AppendLine("[assembly: Guid(\"a251bafb-a13c-4749-b38e-b183da32bf21\")]");
			builder.AppendLine("[assembly: AssemblyVersion(\"1.0.0.0\")]");
			builder.AppendLine("[assembly: AssemblyFileVersion(\"1.0.0.0\")]");

			builder.AppendFormat("[assembly: PluginId(\"Tailviewer.Systemtest\", \"{0}\")]", pluginName);
			builder.AppendLine();

			builder.AppendLine("[assembly: PluginVersion(0, 0)]");

			var fileName = Path.Combine(folder, "AssemblyInfo.cs");
			File.WriteAllText(fileName, builder.ToString());
			return fileName;
		}

		private static IReadOnlyList<string> DownloadDependencies(string folder, string tailviewerVersion)
		{
			return new Nuget().Download(folder, tailviewerVersion);
		}

		private static string GenerateProjectFile(string pluginName,
		                                          string folder,
		                                          string packageConfig,
		                                          IReadOnlyList<string> sourceFiles,
		                                          IReadOnlyList<string> dependencies,
		                                          out string outputPath)
		{
			var builder = new StringBuilder();

			builder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			builder.AppendLine("<Project ToolsVersion=\"15.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
			builder.AppendLine("\t<Import Project=\"$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props\" Condition=\"Exists(\'$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props\')\" />");
			builder.AppendLine("\t<PropertyGroup>");
			builder.AppendLine("\t\t<Configuration Condition=\" \'$(Configuration)\' == \'\' \">Debug</Configuration>");
			builder.AppendLine("\t\t<Platform Condition=\" \'$(Platform)\' == \'\' \">AnyCPU</Platform>");
			builder.AppendLine("\t\t<ProjectGuid>{A251BAFB-A13C-4749-B38E-B183DA32BF21}</ProjectGuid>");
			builder.AppendLine("\t\t<OutputType>Library</OutputType>");
			builder.AppendLine("\t\t<AppDesignerFolder>Properties</AppDesignerFolder>");
			builder.AppendLine("\t\t<RootNamespace>Tailviewer.PluginCreator</RootNamespace>");
			builder.AppendFormat("\t\t<AssemblyName>{0}</AssemblyName>", pluginName);
			builder.AppendLine("\t\t<TargetFrameworkVersion>v4.5.2</TargetFrameworkVersion>");
			builder.AppendLine("\t\t<FileAlignment>512</FileAlignment>");
			builder.AppendLine("\t\t<TargetFrameworkProfile />");
			builder.AppendLine("\t</PropertyGroup>");
			builder.AppendLine("\t<PropertyGroup Condition=\" \'$(Configuration)|$(Platform)\' == \'Release|AnyCPU\' \">");
			builder.AppendFormat("\t\t<OutputPath>{0}</OutputPath>", folder);
			builder.AppendLine();
			builder.AppendLine("\t</PropertyGroup>");
			builder.AppendLine("\t<ItemGroup>");
			builder.AppendLine("\t\t<Reference Include=\"System\" />");
			builder.AppendLine("\t\t<Reference Include=\"System.Core\" />");
			foreach (var dependency in dependencies)
			{
				var assemblyFileName = Path.GetFileName(dependency);
				var assemblyName = Path.GetFileNameWithoutExtension(assemblyFileName);

				builder.AppendFormat("\t\t<Reference Include=\"{0}\">", assemblyName);
				builder.AppendLine();
				builder.AppendFormat("\t\t\t<HintPath>{0}</HintPath>", dependency);
				builder.AppendLine();
				builder.AppendLine("\t\t</Reference>");
			}
			builder.AppendLine("\t</ItemGroup>");
			builder.AppendLine("\t<ItemGroup>");
			foreach (var sourceFile in sourceFiles)
			{
				builder.AppendFormat("\t\t<Compile Include=\"{0}\"></Compile>", sourceFile);
			}
			builder.AppendLine("\t</ItemGroup>");
			builder.AppendLine("\t<ItemGroup>");
			builder.AppendFormat("\t\t<None Include=\"{0}\"></None>", packageConfig);
			builder.AppendLine();
			builder.AppendLine("\t</ItemGroup>");
			builder.AppendLine("\t<Import Project=\"$(MSBuildToolsPath)\\Microsoft.CSharp.targets\" />");
			builder.AppendLine("</Project>");

			var projectFileName = Path.Combine(folder, "Plugin.csproj");
			File.WriteAllText(projectFileName, builder.ToString());

			outputPath = Path.Combine(folder, $"{pluginName}.dll");

			return projectFileName;
		}

		private static void BuildProject(string projectFile)
		{
			new MsBuild().Build(projectFile, "release");
		}

		private static string PackPluginAssembly(string folder, string assembly, string pluginName)
		{
			return new Packer().Pack(folder, assembly, pluginName);
		}

		private static IReadOnlyList<Type> ResolveInterfaceType(string pluginNames)
		{
			var typeNames = pluginNames.Split(',');
			var types = new List<Type>();
			foreach (var name in typeNames)
			{
				types.Add(typeof(IPlugin).Assembly.GetType(name));
			}

			return types;
		}
	}
}
