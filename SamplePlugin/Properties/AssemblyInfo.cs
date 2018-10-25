using System.Reflection;
using System.Runtime.InteropServices;
using Tailviewer.BusinessLogic.Plugins;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SamplePlugin")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("SamplePlugin")]
[assembly: AssemblyCopyright("Copyright © Simon Miessler 2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("c2c093cc-86f9-4a39-afc1-bf2d54b7f856")]

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
[assembly: AssemblyVersion("0.7.0.0")]
[assembly: AssemblyFileVersion("0.7.0.0")]

// You want to pick some really unique - otherwise you risk getting your plugin not
// loaded because Tailviewer will only load one plugin per id (the one with the highest version).
[assembly: PluginId("Kittyfisto", "SamplePlugin")]
[assembly: PluginDescription("A skeleton plugin implementation")]
[assembly: PluginAuthor("Simon")]
[assembly: PluginWebsite("https://github.com/Kittyfisto/Tailviewer")]
