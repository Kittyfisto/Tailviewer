# Tailviewer Plugin Repository

This application is a standalone command-line application which which can be used to distribute tailviewer plugins.

## Example

1. Deploy the repository on a server named <server>
2. Execute ```> repository.exe run```
3. Start tailviewer
4. Go to settings
6. Enter the following address under plugin repository: ```tvpr://<server>:1234```
7. Go to plugins
8. Press update

Tailviewer should tell you that all plugins are up-to-date.

## Overview

The plugin repository enables interested parties to distribute their own tailviewer plugins to clients without having to put them in some central, globally available
repository. This could be for a multitude of reasons, for example they might not want to publish information on how to analyze their log files.

While it is certainly possible to simply ship plugins by copying them onto file shares or sending them via email, it can become quite exhausting if updates
are sent every week. By using a private plugin repository instead, plugin publishing and updating can be completely automated.

## Running the repository

Running a repository requires nothing more but ```> repository run```.
The newly started process will bind to 0.0.0.0:1234 by default and serve requests to every client.
The repository allows several options to be configured.
An example configuration can be created with ```> repository write-configuration``` and to use a particular file, simply use
```> repository run --configuration <configuration file path>```.

## User management

Publishing plugins (via command-line or remote via archive.exe) to a repository requires an access token of a user of said repository.
The list of users can be obtained via ```> repository list-users```
New users can be added via ```> repository add-user <username> <email>```
Users can be removed via ```> repository remove-user <username>```

Once a new user is added, a unique access token will be generated for said user. This token will then have to be used to publish plugins to the repository.

## Publishing plugins

There are currently two ways to publish plugins to a repository:

### Publishing on the repository machine itself

```> repository.exe add-plugin MyPlugin.0.1.tvp --user <username>```
OR
```> repository.exe add-plugin MyPlugin.0.1.tvp --access-token <access-token>```

### Publish remotely via archive.exe

```> archive.exe publish MyPlugin.0.1.tvp --repository tvpr://<server>:1234 --access-token <access-token>```

Publishing remotely is the only way to publish plugins to a repository on a different machine, however it is disabled by default.
The repository needs to enable remote publishing either via a command-line option to run or by changing the configuration file
given to run.

## Internals

Both users and plugins are stored in an sqlite database under `%LocalAppData%\Tailviewer.PluginRepository\Plugins.isdb`.
However most information cannot be inspected with a simple sqlite browser because serialized .NET objects are stored in the database.
Currently, the best option to inspect the database is via the repository command-line which allows both inspection of plugins
as well as users.

## Migrating the database to a newer version

The following steps need to be performed when updating to a new major version or when a breaking change
during the beta phase was performed:

1. Export the repository (using the OLD repository.exe) ```> repository export C:\SomeFolder```
2. Delete (or move) the existing repository `%LocalAppData%\Tailviewer.PluginRepository\Plugins.isdb`
3. Install the newer plugin repository version
4. Execute the batch file in `C:\SomeFolder` to re-create the database
