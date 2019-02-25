# Developing analysis plugins

## Why?

You want to develop an analysis plugin when you want tailviewer:
- To support a particular analysis (for example a special aggregation) which it doesn't already support

## Where?

TODO

## How?

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
