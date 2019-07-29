# Tailviewer Plugin Repository

This application is a standalone command-line application as well as a windows service which which can be used to distribute tailviewer plugins.

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

There are currently two different ways to run the repository. You can chose either of them (whichever floats your boat).

### Running the repository as a command-line application

Running a repository requires nothing more but ```> repository run```.
The newly started process will bind to 0.0.0.0:1234 by default and serve requests to every client.
The application will use the configuration file under ```%ALLUSERSPROFILE%\Tailviewer.PluginRepository\configuration.xml``` and create
one if none exists. Users can change this file to their needs (for example binding the repository to a custom adapter / port).
```> repository run``` logs to ```%ALLUSERSPROFILE%\Tailviewer.PluginRepository\repository.log```.

### Running the repository as a windows service

Alternatively it's possible to run the repository as a windows service.
Simply call `start_service.cmd` to create (if necessary) and then start the repository.
The service is started automatically and restarted in case it crashes.
Just like ```repository.exe```, the newly started service will bind to 0.0.0.0:1234 by default and serve requests to every client
and can be configured via the same configuration file under ```%ALLUSERSPROFILE%\Tailviewer.PluginRepository\configuration.xml```.
```repository-svc``` logs to ```%ALLUSERSPROFILE%\Tailviewer.PluginRepository\repository-svc.log```.

## Maintaining the repository

The following section shows you how to maintain the repository (user & plugin management). All of the following steps work
regardless of which method was chosen to run the repository.

### User management

Publishing plugins (via command-line or remote via archive.exe) to a repository requires an access token of a user of said repository.
The list of users can be obtained via ```> repository list-users```
New users can be added via ```> repository add-user <username> <email>```
Users can be removed via ```> repository remove-user <username>```

Once a new user is added, a unique access token will be generated for said user. This token will then have to be used to publish plugins to the repository.  

## Publishing plugins

There are currently two ways to publish plugins to a repository:

#### Publishing on the repository machine itself

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

1. Download the newest repository 'Tailviewer-Repository.x.y.z.zip'
2. Unzip the entire contents into a temporary directory
3. Start a shell in that directory **with administrator rights**
4. Execute install.cmd
5. Enjoy a beverage of your choice
6. Delete the temporarily created folder
