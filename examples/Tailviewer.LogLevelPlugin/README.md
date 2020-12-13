# Custom log level parsing plugin example

This example plugin implementation shows how you are able to change the way tailviewer treats plugins
of particular (or all) log files by overwriting the default ILogLineTranslator implementation.
There are two steps required to make this work:
1. An implementation ILogLineTranslator (done by `MyCustomLogLevelTranslator` in this example) which takes care of parsing
the custom log levels from the log file and mapping them to tailviewer's well known levels (trace, debug, info, warning, error and fatal)  
2. An implementation of IFileFormatPlugin (or IFileFormatPlugin2) has to be created which allows you to specify
which log files are modified. It is possible to overwrite the behavior for each and every log file, for log files with
a particular file extension or for log file's who's name matches a particular regular expression. This implementation
is also responsible for overwriting the default implementation of `ILogLineTranslator` with the custom one created in step 1.  

Upon loading this plugin, tailviewer will call IFileFormatPlugin.Open() whenever a log file with a matching filename is opened,
thus ensuring that the log file's lines are translated as desired.
