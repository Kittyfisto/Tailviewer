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
and create a new Class Library targeting at least .NET 4.5.2.
You must then add a reference to Tailviewer.API which can be found on [nuget.org](https://www.nuget.org/packages/tailviewer.api/).
Please note the API is not yet stable and subject to change until release 1.0.  
Once you've implemented at least one of the IPlugin interfaces (such as IFileFormatPlugin),
you can place your plugin in the "%ProgramFiles%\Tailviewer\Plugins" folder.

## Adding support for a custom file format

See [developing file format plugins](DevelopingFileFormatPlugins.md).

## Adding new analyses for a Tailviewer analysis

See [developing analysis plugins](DevelopingAnalysisPlugins.md).

## Adding new visualizations for a Tailviewer analysis

See [developing widget plugins](DevelopingWidgetPlugins.md).
