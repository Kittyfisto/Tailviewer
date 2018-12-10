# Developing a file format plugin

## Why?

You want to develop a file format plugin when you want tailviewer to
- Display a (most likely proprietary) file which isn't in text form
- Modify how a particular text file is displayed

## Where is it used?

If you open a particular file with tailviewer (either by opening it directly or by dragging it over tailviewer), then tailviewer will try to 
find a plugin which supports that file format (determined by its file extension). If no plugin is available,
tailviewer will interpret the file as a text file.

## Getting started

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
