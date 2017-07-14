Tailviewer can be extended by installing plugins for file formats which aren't natively supported.

In order to start developing a plugin, you need to install Visual Studio (any version since Visual Studio 2012 will do)
and create a new Class Library targeting .NET 4.5. You must then add a reference to Tailviewers API which can be found on https://www.nuget.org/packages/tailviewer.api/.
Please note the API is not yet stable and subject to change until release 1.0. Once you've implemented at least one of the IPlugin interfaces,
such as IFileFormatPlugin, you can place your plugin in this folder.

A real world example of such a plugin can be found under https://github.com/Kittyfisto/Tailviewer.Plugins.SQLite,
which shows how to allow Tailviewer to display a table from a SQLite file.