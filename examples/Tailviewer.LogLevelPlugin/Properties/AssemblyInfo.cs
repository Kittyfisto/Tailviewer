using System.Reflection;
using System.Runtime.InteropServices;
using Tailviewer.Plugins;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("LogLevelPlugin")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyProduct("LogLevelPlugin")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a628892a-4be2-4726-983e-cce47c716cc1")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: PluginVersion(1, 0, 0, 0)]
[assembly: PluginWebsite("https://github.com/Kittyfisto/Tailviewer/tree/master/examples/Tailviewer.LogLevelPlugin")]
[assembly: PluginId("Tailviewer", "LogLevelPlugin")]
[assembly: PluginAuthor("Simon Mießler")]
[assembly: PluginDescription("Shows how a plugin can alter tailviewer's behavior to detect different log levels")]
