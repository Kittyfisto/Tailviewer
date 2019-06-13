# Developing plugins

Plugins are a way for you to progrmmatically extend Tailviewer.

## Overview

Plugins...

- Are a zip archive containing a description of the plugin (author, website, version, description and all that jazz)
- Consist of at least one .NET assembly which is loaded by Tailviewer upon startup
- Implement at least one of the available `IPlugin` interfaces: `IFileFormatPlugin`, `ILogAnalyserPlugin` or `IWidgetPlugin`
- Can add support for custom file formats or even more exotic datasources
- Can add new visualizations to Tailviewer's analysis feature
- Can add new analyses to Tailviewer's analysis feature

## Getting started

In order to start developing a plugin, you need to install Visual Studio (any version since Visual Studio 2012 will do)
and create a new Class Library targeting at least .NET 4.5.2:

![Creating a project in Visual Studio](CreateProject.png?raw=true)

Once created, you need to add a reference to Tailviewer.API. You can download this package manually from [nuget.org](https://www.nuget.org/packages/tailviewer.api/) or
you can install it in Visual Studio by right clicking your project in the Solution Explorer and clicking "Managed Nuget Packages...". Make sure
to select "Include prerelease".

![Installing the nuget package in Visual Studio](NugetPackage.png?raw=true)

Your project should now compile, however there's two highly recommended additional steps you should do:

The nuget package comes with a build tool called "archive.exe" which responsible for automatically creating a zip archive from your assembly and its dependencies.
You can make use of it by right clicking your project in the Solution Explorer once again, this time selecting "Properties". Then go to "Build Events" and enter the
following in the "Post-build event command line" box:

```
"$(MSBuildProjectDirectory)\packages\Tailviewer.Api.0.7.2.630-beta\build\archive.exe" pack "$(MSBuildProjectDirectory)\$(OutDir)MyFirstTailviewerPlugin.dll"
xcopy "$(MSBuildProjectDirectory)\$(OutDir)MyFirstTailviewerPlugin.*.tvp" "%ProgramW6432%\Tailviewer\Plugins\" /y
```

![Configuring the post-build event](BuildEvents.png?raw=true)

This will cause archive.exe to create a plugin from your assembly and also copies it into Tailviewer's plugin directory. Please note that in order to do the latter, Visual Studio needs to be started
with administrator privileges because %ProgramW6432% is protected by UAC.  archive.exe will add warnings and errors to the Build output of Visual Studio, if necessary. These should be self-explanatory,
but if they're not, consider creating an issue for it.

Example warnings:
```
1>EXEC : warning : Plugin 'bin\Debug\SomePlugin.dll' is missing the PluginAuthor attribute, please consider adding it
1>EXEC : warning : Plugin 'bin\Debug\SomePlugin.dll' is missing the PluginWebsite attribute, please consider adding it
1>EXEC : warning : Plugin 'bin\Debug\SomePlugin.dll' is missing the PluginDescription attribute, please consider adding it
1>EXEC : warning : Plugin 'bin\Debug\SomePlugin.dll' is missing the PluginVersion attribute, please consider adding it. Defaulting to 0.0.0
```

Finally, in order to simplify debugging, you should install the latest Tailviewer build and then configure Visual Studio to automatically start Tailviewer when you start debugging:
To do so, right click your project in the Solution Explorer once more, selecting "Properties". Then go to "Debug" and select "Start external program" and enter the following path:

```
C:\Program Files\Tailviewer\Tailviewer.exe
```

![Configuration debugger](DebugSettings.png?raw=true)

You can now debug your plugin by "Selecting Debug => Start Debugging" (which defaults to F5 on my computer, but ymmv) and it will start Tailviewer which will in turn load your plugin.

## Adding support for a custom file format

See [developing file format plugins](DevelopingFileFormatPlugins.md).

## Adding new analyses for a Tailviewer analysis

See [developing analysis plugins](DevelopingAnalysisPlugins.md).

## Adding new visualizations for a Tailviewer analysis

See [developing widget plugins](DevelopingWidgetPlugins.md).
