# Tailviewer

[![Build status](https://ci.appveyor.com/api/projects/status/mripd18s222ue6gm?svg=true)](https://ci.appveyor.com/project/Kittyfisto/sharptail)
![AppVeyor tests](https://img.shields.io/appveyor/tests/Kittyfisto/sharptail.svg?color=%234CC61E)
![GitHub release](https://img.shields.io/github/release/kittyfisto/tailviewer.svg)

Tailviewer is a free and active open-source log file viewer.

Tailviewer is supported on Windows 7, 8 and 10 and requires [.NET 4.7.1](https://www.microsoft.com/en-us/download/details.aspx?id=56116) or higher.

Head on over to https://kittyfisto.github.io/Tailviewer/ in order to download the newest version.

![Tailviewer application](/Screenshot1.png?raw=true)  

Tailviewer supports live filtering on medium to large (< 1Gb) log files.

![Live filtering](/Screenshot2.png?raw=true)

## Features

- Multiline log entries
- Searching
- Filtering (substring, wildcard & regexp)
- Filter by timestamp range
- Filtering by log level (fatal, error, warning, info and debug)
- Highlighting by log level
- Merging multiple files (by timestamp)
- Bookmarks
- Additional columns:
   - Elapsed time between log entries
   - Elapsed time since first log entry
- Plugin system to support custom/proprietary formats (submitting an issue/mr is also an option, if I have the time)

## Installation

Simply download the latest **stable** version from https://kittyfisto.github.io/Tailviewer/.  
You could also try out the latest "nightly" build from here: https://ci.appveyor.com/project/Kittyfisto/sharptail

## Developing plugins

Tailviewer can be extended by installing plugins for file formats which aren't natively supported.

You can find a more detailed of how to write plugins [here](docs/DevelopingPlugins.md).

## Contributing

See [here](CONTRIBUTING.md)

## History

TODO: Write history

## Credits

TODO

## License

[MIT](http://opensource.org/licenses/MIT)
