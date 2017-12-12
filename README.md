# Tailviewer

[![Build status](https://ci.appveyor.com/api/projects/status/mripd18s222ue6gm?svg=true)](https://ci.appveyor.com/project/Kittyfisto/sharptail)  

Tailviewer is a free and active open-source log file viewer.

Tailviewer is supported on Windows 7, 8 and 10 and requires .NET 4.5 or higher.

Head on over to https://kittyfisto.github.io/Tailviewer/ in order to download the newest version.

![Tailviewer application](/Screenshot1.png?raw=true)  

Tailviewer supports live filtering on medium to large (< 1Gb) log files.

![Live filtering](/Screenshot2.png?raw=true)

## Installation

Simply download the latest version from https://kittyfisto.github.io/Tailviewer/.

## Developing plugins

Tailviewer can be extended by installing plugins for file formats which aren't natively supported.

In order to start developing a plugin, you need to install Visual Studio (any version since Visual Studio 2012 will do) and create a new Class Library targeting .NET 4.5. You must then add a reference to Tailviewers API which can be found on [nuget.org](https://www.nuget.org/packages/tailviewer.api/). Please note the API is not yet stable and subject to change until release 1.0.  
Once you've implemented at least one of the IPlugin interfaces (such as IFileFormatPlugin), you can place your plugin in the "%ProgramFiles%\Tailviewer\Plugins" folder.

A real world example of such a plugin can be found [here](https://github.com/Kittyfisto/Tailviewer.Plugins.SQLite), which shows how to allow Tailviewer to display a table from a SQLite file.

## Contributing

See [here](CONTRIBUTING.md)

## History

TODO: Write history

## Credits

TODO

## License

[MIT](http://opensource.org/licenses/MIT)
