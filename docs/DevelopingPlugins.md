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

If you want to support a custom file format which isn't supported by Tailviewer natively, then you need to implement the IFileFormatPlugin interface.
The following is an example implementation to support files with a *.foo extension:

```csharp
public class FooFileFormatPlugin : IFileFormatPlugin
{
   public IReadOnlyList<string> SupportedExtensions => new[]{".foo"};
   public ILogFile Open(string fileName, ITaskScheduler taskScheduler)
   {
       return new FooLogFile(fileName, taskScheduler);
   }
}
```

Where `FooLogFile` is an implementation of `ILogFile`:

```csharp
public class FooLogFile : ILogFile
{
}
```

Implementing `ILogFile` can be quite daunting as you will have to take care of:
- Notifying Tailviewer (through an `ILogFileListener`) when changes to the data source are performed (for example when an event is appended or the data source is cleared alltogether)
- Provide read-only access to various subsets of the data source (for example events #100-199)

A real world example of such a plugin can be found [here](https://github.com/Kittyfisto/Tailviewer.Plugins.SQLite), which shows how to allow Tailviewer to display a table from an SQLite file.

## Adding new analyses for a Tailviewer analysis

Adding a new analysis can be done by implementing four core interfaces:
- `ILogAnalyserPlugin`
- `ILogAnalyserConfiguration` 
- `ILogAnalyser`
- `ILogAnalysisResult`

The first serves as a factory to create new analysers for a particular data source and configuration, the second serves as a way for the user to forward a configuration to the analyser (can be skipped if the analysis doesn't have a configuration) and the third does the analysis itself. The fourth class serves as a way to hold the result of the analysis itself and is transported back to the UI to be displayed by a widget.

```csharp
public class MyLogAnalyserPlugin : ILogAnalyserPlugin
{
   public LogAnalyserFactoryId Id => new LogAnalyserFactoryId("MyLogAnalyserPlugin");
   public IEnumerable<KeyValuePair<string, Type>> SerializableTypes => new Dictionary<string, Type>{ { "MyLogAnalyserConfiguration", typeof(MyLogAnalyserConfiguration) } };
   public ILogAnalyser Create(ITaskScheduler scheduler,
			ILogFile source,
			ILogAnalyserConfiguration configuration)
   {
      return new MyLogAnalyser(scheduler, source, (MyLogAnalyserConfiguration)configuration);
   }
}
```

```csharp
public class MyLogAnalyserConfiguration : ILogAnalyserConfiguration
{
	public object Clone()
	{
		return new MyLogAnalyserConfiguration();
	}

	public bool IsEquivalent(ILogAnalyserConfiguration other)
	{
		return false;
	}

	public void Serialize(IWriter writer)
	{}

	public void Deserialize(IReader reader)
	{}
}
```

```csharp
class MyLogAnalyser : ILogAnalyser, ILogFileListener
{
	public MyLogAnalyser(ITaskScheduler scheduler,
			ILogFile source,
			ILogAnalyserConfiguration configuration)
	{
		_source = source;
		_source.AddListener(this, TimeSpan.FromSeconds(1), maximumLineCount: 1000);
	}
	
	public void OnLogFileModified(ILogFile logFile, LogFileSection section)
	{
		// Called whenever a particular section of the log file was modified
	}
	
	public void Dispose()
	{
		_source.RemoveListener(this);
	}
}
```

An Implementation of `ILogAnalyser` must do the following:
- React to changes of its `ILogFIle` source
- Analyse the log file
- Provide an `ILogAnalysisResult` object which holds the analaysis result

Such an implementation should be aware that a data source:
- May be appended
- May be invalidated in parts
- May be cleared
at any time. A naive implementation may run the entire analysis from start to finish every time upon every modification and this may work well enough until the data source reaches a certain time / the analysis reaches a particular complexity. If this is an issue for you, then the implementation will need to update the analysis for the invalidated parts only, to improve performance. How this can be achieved depends on the type of analysis and cannot be generalized. You can, however, draw inspiration from existing plugins `Tailviewer.Count`, `Tailviewer.Events` and `Tailviewer.QuickInfo`, all of which are checked into this repository.

## Adding new visualizations for a Tailviewer analysis

TODO
